using CheltuieliApp.Data;
using CheltuieliApp.DTOs;
using CheltuieliApp.Models;

namespace CheltuieliApp.Services;

public class CategoryService
{
    private readonly AppDatabase _database;

    public CategoryService(AppDatabase database)
    {
        _database = database;
    }

    public async Task EnsureDefaultsAsync()
    {
        var existingCategories = await _database.Db.Table<CategoryEntity>().ToListAsync();

        if (!existingCategories.Any())
        {
            var categories = new List<CategoryEntity>
            {
                new() { Name = "Supermarket", Type = "Expense", ColorHex = "#22C55E", Icon = "cart" },
                new() { Name = "Restaurante / Cafenele", Type = "Expense", ColorHex = "#F97316", Icon = "coffee" },
                new() { Name = "Transport", Type = "Expense", ColorHex = "#3B82F6", Icon = "car" },
                new() { Name = "Sănătate", Type = "Expense", ColorHex = "#EF4444", Icon = "health" },
                new() { Name = "Abonamente", Type = "Expense", ColorHex = "#8B5CF6", Icon = "subscription" },
                new() { Name = "Shopping", Type = "Expense", ColorHex = "#EC4899", Icon = "shopping" },
                new() { Name = "Educație", Type = "Expense", ColorHex = "#6366F1", Icon = "book" },
                new() { Name = "Divertisment", Type = "Expense", ColorHex = "#F59E0B", Icon = "game" },
                new() { Name = "Investiții", Type = "Expense", ColorHex = "#0F766E", Icon = "chart" },
                new() { Name = "Transferuri", Type = "Both", ColorHex = "#64748B", Icon = "transfer" },
                new() { Name = "Retrageri numerar", Type = "Expense", ColorHex = "#78716C", Icon = "cash" },
                new() { Name = "Venituri", Type = "Income", ColorHex = "#16A34A", Icon = "income" },
                new() { Name = "Bonusuri", Type = "Income", ColorHex = "#84CC16", Icon = "gift" },
                new() { Name = "Altele", Type = "Both", ColorHex = "#9CA3AF", Icon = "other" }
            };

            foreach (var category in categories)
                await _database.Db.InsertAsync(category);
        }

        await EnsureDefaultRulesAsync();
    }

    private async Task EnsureDefaultRulesAsync()
    {
        var existingRules = await _database.Db.Table<MerchantRuleEntity>().ToListAsync();

        if (existingRules.Any())
            return;

        var categories = await _database.Db.Table<CategoryEntity>().ToListAsync();

        int CategoryId(string name)
        {
            return categories.First(x => x.Name == name).Id;
        }

        var rules = new List<MerchantRuleEntity>
        {
            // Supermarket
            new() { Keyword = "AUCHAN", CategoryId = CategoryId("Supermarket") },
            new() { Keyword = "LIDL", CategoryId = CategoryId("Supermarket") },
            new() { Keyword = "PROFI", CategoryId = CategoryId("Supermarket") },
            new() { Keyword = "MEGA IMAGE", CategoryId = CategoryId("Supermarket") },
            new() { Keyword = "CARREFOUR", CategoryId = CategoryId("Supermarket") },
            new() { Keyword = "KAUFLAND", CategoryId = CategoryId("Supermarket") },
            new() { Keyword = "TERESY", CategoryId = CategoryId("Supermarket") },

            // Restaurante / cafenele
            new() { Keyword = "SISTERS CAFE", CategoryId = CategoryId("Restaurante / Cafenele") },
            new() { Keyword = "INM CJ IULIUS", CategoryId = CategoryId("Restaurante / Cafenele") },
            new() { Keyword = "MCDONALD", CategoryId = CategoryId("Restaurante / Cafenele") },
            new() { Keyword = "KFC", CategoryId = CategoryId("Restaurante / Cafenele") },
            new() { Keyword = "STARBUCKS", CategoryId = CategoryId("Restaurante / Cafenele") },

            // Transport
            new() { Keyword = "BOLT", CategoryId = CategoryId("Transport") },
            new() { Keyword = "UBER", CategoryId = CategoryId("Transport") },
            new() { Keyword = "BILETERIA", CategoryId = CategoryId("Transport") },
            new() { Keyword = "AUTOGARI", CategoryId = CategoryId("Transport") },

            // Sănătate
            new() { Keyword = "SYNEVO", CategoryId = CategoryId("Sănătate") },
            new() { Keyword = "REMEDIUM", CategoryId = CategoryId("Sănătate") },
            new() { Keyword = "FARM", CategoryId = CategoryId("Sănătate") },
            new() { Keyword = "PHARM", CategoryId = CategoryId("Sănătate") },

            // Abonamente
            new() { Keyword = "SPOTIFY", CategoryId = CategoryId("Abonamente") },
            new() { Keyword = "NETFLIX", CategoryId = CategoryId("Abonamente") },
            new() { Keyword = "OPENAI", CategoryId = CategoryId("Abonamente") },
            new() { Keyword = "CHATGPT", CategoryId = CategoryId("Abonamente") },
            new() { Keyword = "GEOGUESSR", CategoryId = CategoryId("Abonamente") },

            // Shopping
            new() { Keyword = "EMAG", CategoryId = CategoryId("Shopping") },
            new() { Keyword = "FASHION", CategoryId = CategoryId("Shopping") },
            new() { Keyword = "MI.COM", CategoryId = CategoryId("Shopping") },

            // Divertisment
            new() { Keyword = "SUPERCELL", CategoryId = CategoryId("Divertisment") },

            // Investiții
            new() { Keyword = "BT OBLIGATIUNI", CategoryId = CategoryId("Investiții") },
            new() { Keyword = "BT INDEX ROMANIA", CategoryId = CategoryId("Investiții") },
            new() { Keyword = "ACHIZITIE UNITATI DE FOND", CategoryId = CategoryId("Investiții") },

            // Transferuri
            new() { Keyword = "TRANSFER INTRE CONTURILE PROPRII", CategoryId = CategoryId("Transferuri") },
            new() { Keyword = "TRANSFER BT PAY", CategoryId = CategoryId("Transferuri") },
            new() { Keyword = "P2P BTPAY", CategoryId = CategoryId("Transferuri") },
            new() { Keyword = "PLATA INSTANT", CategoryId = CategoryId("Transferuri") },

            // Retrageri / numerar
            new() { Keyword = "RETRAGERE NUMERAR", CategoryId = CategoryId("Retrageri numerar") },
            new() { Keyword = "RETRAGERI DE NUMERAR", CategoryId = CategoryId("Retrageri numerar") },

            // Venituri
            new() { Keyword = "UNIVERSITATEA BABES BOLYAI", CategoryId = CategoryId("Venituri") },
            new() { Keyword = "TRANSFER CREDIT", CategoryId = CategoryId("Venituri") },
            new() { Keyword = "INCASARE", CategoryId = CategoryId("Venituri") },
            new() { Keyword = "DEPUNERE NUMERAR", CategoryId = CategoryId("Venituri") },

            // Bonusuri
            new() { Keyword = "BONUS", CategoryId = CategoryId("Bonusuri") }
        };

        foreach (var rule in rules)
            await _database.Db.InsertAsync(rule);
    }

    public async Task<List<CategoryEntity>> GetCategoriesAsync()
    {
        return await _database.Db
            .Table<CategoryEntity>()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task ApplyCategoriesAsync(List<BankTransactionDto> transactions)
    {
        await EnsureDefaultsAsync();

        var categories = await _database.Db.Table<CategoryEntity>().ToListAsync();
        var rules = await _database.Db.Table<MerchantRuleEntity>().ToListAsync();

        foreach (var transaction in transactions)
        {
            var searchableText =
                $"{transaction.Merchant} {transaction.Description} {transaction.RawText}"
                .ToUpperInvariant();

            var matchedRule = rules
                .FirstOrDefault(rule =>
                    searchableText.Contains(rule.Keyword.ToUpperInvariant()));

            if (matchedRule == null)
            {
                transaction.CategoryId = null;
                transaction.CategoryName = "";
                continue;
            }

            var category = categories.FirstOrDefault(x => x.Id == matchedRule.CategoryId);

            if (category == null)
                continue;

            transaction.CategoryId = category.Id;
            transaction.CategoryName = category.Name;
        }
    }

    public async Task AddMerchantRuleAsync(string keyword, int categoryId, string bank = "")
    {
        keyword = keyword.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(keyword))
            return;

        var existing = await _database.Db.Table<MerchantRuleEntity>()
            .FirstOrDefaultAsync(x =>
                x.Keyword.ToUpper() == keyword &&
                x.Bank == bank);

        if (existing != null)
            return;

        await _database.Db.InsertAsync(new MerchantRuleEntity
        {
            Keyword = keyword,
            CategoryId = categoryId,
            Bank = bank
        });
    }
}