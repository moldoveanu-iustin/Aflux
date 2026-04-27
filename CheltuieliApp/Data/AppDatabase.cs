using SQLite;
using CheltuieliApp.Models;

namespace CheltuieliApp.Data;

public class AppDatabase
{
    private readonly SQLiteAsyncConnection _db;

    public AppDatabase(string dbPath)
    {
        _db = new SQLiteAsyncConnection(dbPath);

        _db.CreateTableAsync<StatementImportEntity>().Wait();
        _db.CreateTableAsync<TransactionEntity>().Wait();
        _db.CreateTableAsync<CategoryEntity>().Wait();
        _db.CreateTableAsync<MerchantRuleEntity>().Wait();
    }

    public SQLiteAsyncConnection Db => _db;
}