using CheltuieliApp.Data;
using CheltuieliApp.DTOs;
using CheltuieliApp.Models;

namespace CheltuieliApp.Services;

public class ImportService
{
    private readonly AppDatabase _database;

    public ImportService(AppDatabase database)
    {
        _database = database;
    }

    public async Task SaveStatementAsync(BankStatementDto dto, string fileName, string fileHash)
    {
        var import = new StatementImportEntity
        {
            Bank = dto.Bank,
            AccountIban = dto.AccountIban,
            PeriodStart = dto.PeriodStart,
            PeriodEnd = dto.PeriodEnd,
            TransactionCount = dto.Transactions.Count,
            FileName = fileName,
            FileHash = fileHash
        };

        await _database.Db.InsertAsync(import);

        await Shell.Current.DisplayAlertAsync(
                "Debug",
                $"Import ID: {import.Id}",
                "OK");

        foreach (var t in dto.Transactions)
        {
            var entity = new TransactionEntity
            {
                StatementImportId = import.Id,
                Bank = t.Bank,
                AccountIban = t.AccountIban,
                TransactionDate = t.TransactionDate,
                Amount = t.Amount,
                Direction = t.Direction,
                Merchant = t.Merchant,
                Description = t.Description
            };

            await _database.Db.InsertAsync(entity);
        }
    }
    public async Task<List<StatementImportEntity>> GetImportsAsync()
    {
        return await _database.Db
            .Table<StatementImportEntity>()
            .OrderByDescending(x => x.ImportedAt)
            .ToListAsync();
    }
    public async Task<List<TransactionEntity>> GetTransactionsForImportAsync(int statementImportId)
    {
        return await _database.Db
            .Table<TransactionEntity>()
            .Where(x => x.StatementImportId == statementImportId)
            .OrderBy(x => x.TransactionDate)
            .ToListAsync();
    }
    public async Task DeleteImportAsync(int statementImportId)
    {
        var transactions = await _database.Db
            .Table<TransactionEntity>()
            .Where(x => x.StatementImportId == statementImportId)
            .ToListAsync();

        foreach (var transaction in transactions)
            await _database.Db.DeleteAsync(transaction);

        var import = await _database.Db
            .Table<StatementImportEntity>()
            .FirstOrDefaultAsync(x => x.Id == statementImportId);

        if (import != null)
            await _database.Db.DeleteAsync(import);
    }
}