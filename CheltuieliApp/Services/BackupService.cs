using System.Text.Json;
using CheltuieliApp.DTOs;
using CheltuieliApp.Data;
using CheltuieliApp.Models;

namespace CheltuieliApp.Services;

public class BackupService
{
    private readonly AppDatabase _database;

    public BackupService(AppDatabase database)
    {
        _database = database;
    }

    public async Task<string> ExportBackupAsync()
    {
        var backup = new BackupDto
        {
            StatementImports = await _database.Db.Table<StatementImportEntity>().ToListAsync(),
            Transactions = await _database.Db.Table<TransactionEntity>().ToListAsync(),
            Categories = await _database.Db.Table<CategoryEntity>().ToListAsync(),
            MerchantRules = await _database.Db.Table<MerchantRuleEntity>().ToListAsync()
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var fileName = $"aflux_backup_{DateTime.Now:yyyyMMdd_HHmm}.json";
        var path = Path.Combine(FileSystem.CacheDirectory, fileName);

        await File.WriteAllTextAsync(path, json);

        return path;
    }

    public async Task ImportBackupAsync(string filePath, bool replaceExisting)
    {
        var json = await File.ReadAllTextAsync(filePath);

        var backup = JsonSerializer.Deserialize<BackupDto>(json);

        if (backup == null)
            throw new Exception("Fișierul de backup nu este valid.");

        if (replaceExisting)
        {
            await _database.Db.DeleteAllAsync<TransactionEntity>();
            await _database.Db.DeleteAllAsync<StatementImportEntity>();
            await _database.Db.DeleteAllAsync<MerchantRuleEntity>();
            await _database.Db.DeleteAllAsync<CategoryEntity>();
        }

        foreach (var category in backup.Categories)
            await _database.Db.InsertOrReplaceAsync(category);

        foreach (var rule in backup.MerchantRules)
            await _database.Db.InsertOrReplaceAsync(rule);

        foreach (var import in backup.StatementImports)
            await _database.Db.InsertOrReplaceAsync(import);

        foreach (var transaction in backup.Transactions)
            await _database.Db.InsertOrReplaceAsync(transaction);
    }
}