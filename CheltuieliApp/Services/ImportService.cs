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

    public async Task SaveStatementAsync(BankStatementDto dto, string fileName, string fileHash, DateTime? allowedStart = null, DateTime? allowedEnd = null)
    {
        var transactions = dto.Transactions.AsEnumerable();

        if (allowedStart.HasValue && allowedEnd.HasValue)
        {
            transactions = transactions.Where(x =>
                x.TransactionDate.Date >= allowedStart.Value.Date &&
                x.TransactionDate.Date <= allowedEnd.Value.Date);
        }

        var transactionsToSave = transactions.ToList();

        if (!transactionsToSave.Any())
            return;

        var import = new StatementImportEntity
        {
            Bank = dto.Bank,
            AccountIban = dto.AccountIban,
            PeriodStart = allowedStart?.Date ?? dto.PeriodStart.Date,
            PeriodEnd = allowedEnd?.Date ?? dto.PeriodEnd.Date,
            TransactionCount = transactionsToSave.Count,
            FileName = fileName,
            FileHash = fileHash,
            ImportedAt = DateTime.Now
        };

        await _database.Db.InsertAsync(import);

        foreach (var t in transactionsToSave)
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
    public async Task<ImportValidationResult> ValidateImportAsync(BankStatementDto statement)
    {
        var existingImports = await _database.Db
            .Table<StatementImportEntity>()
            .Where(x =>
                x.Bank == statement.Bank &&
                x.AccountIban == statement.AccountIban)
            .ToListAsync();

        var overlapping = existingImports
            .Where(x =>
                x.PeriodStart.Date <= statement.PeriodEnd.Date &&
                x.PeriodEnd.Date >= statement.PeriodStart.Date)
            .ToList();

        if (!overlapping.Any())
        {
            return new ImportValidationResult
            {
                Status = ImportValidationStatus.Ok
            };
        }

        var coveredDays = new HashSet<DateTime>();

        foreach (var import in overlapping)
        {
            foreach (var day in EachDay(import.PeriodStart.Date, import.PeriodEnd.Date))
                coveredDays.Add(day);
        }

        var newDays = EachDay(statement.PeriodStart.Date, statement.PeriodEnd.Date).ToList();

        var alreadyCoveredDays = newDays
            .Where(x => coveredDays.Contains(x))
            .OrderBy(x => x)
            .ToList();

        var notCoveredDays = newDays
            .Where(x => !coveredDays.Contains(x))
            .OrderBy(x => x)
            .ToList();

        if (alreadyCoveredDays.Count == newDays.Count)
        {
            return new ImportValidationResult
            {
                Status = ImportValidationStatus.FullyCovered,
                OverlapStart = alreadyCoveredDays.First(),
                OverlapEnd = alreadyCoveredDays.Last(),
                Message =
                    $"Toate zilele din extrasul {statement.PeriodStart:dd.MM.yyyy} - {statement.PeriodEnd:dd.MM.yyyy} " +
                    $"pentru {statement.Bank} sunt deja importate."
            };
        }

        return new ImportValidationResult
        {
            Status = ImportValidationStatus.PartiallyCovered,
            OverlapStart = alreadyCoveredDays.First(),
            OverlapEnd = alreadyCoveredDays.Last(),
            AllowedStart = notCoveredDays.First(),
            AllowedEnd = notCoveredDays.Last(),
            Message =
                $"Anumite zile din extrasul pentru {statement.Bank} sunt deja importate: " +
                $"{alreadyCoveredDays.First():dd.MM.yyyy} - {alreadyCoveredDays.Last():dd.MM.yyyy}.\n\n" +
                $"Doriți să importați doar perioada nouă: " +
                $"{notCoveredDays.First():dd.MM.yyyy} - {notCoveredDays.Last():dd.MM.yyyy}?"
        };
    }
    private static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
    {
        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
            yield return day;
    }


    // Dashboard
    public async Task<List<StatementImportEntity>> GetImportsForMonthAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return await _database.Db
            .Table<StatementImportEntity>()
            .Where(x => x.PeriodStart <= end && x.PeriodEnd >= start)
            .ToListAsync();
    }

    public async Task<List<TransactionEntity>> GetTransactionsForMonthAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        return await _database.Db
            .Table<TransactionEntity>()
            .Where(x => x.TransactionDate >= start && x.TransactionDate < end)
            .ToListAsync();
    }
    public async Task<(decimal Income, decimal Expenses, int Count)> GetMonthlySummaryAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var transactions = await _database.Db.Table<TransactionEntity>()
            .Where(x => x.TransactionDate >= start && x.TransactionDate < end)
            .ToListAsync();

        var income = transactions
            .Where(x => x.Direction == "Credit")
            .Sum(x => x.Amount);

        var expenses = transactions
            .Where(x => x.Direction == "Debit")
            .Sum(x => x.Amount);

        return (income, expenses, transactions.Count);
    }
    public async Task<List<TransactionEntity>> GetTransactionsForDayAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        return await _database.Db
            .Table<TransactionEntity>()
            .Where(x => x.TransactionDate >= start && x.TransactionDate < end)
            .OrderBy(x => x.TransactionDate)
            .ToListAsync();
    }
}