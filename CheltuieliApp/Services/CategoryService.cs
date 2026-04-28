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
        var defaults = new List<CategoryEntity>
    {
        new() { Name = "Supermarket", Type = "Expense", ColorHex = "#22C55E", Icon = "cart", IsSystem = true },
        new() { Name = "Restaurante / Cafenele", Type = "Expense", ColorHex = "#F97316", Icon = "coffee", IsSystem = true },
        new() { Name = "Cadouri", Type = "Expense", ColorHex = "#F52A2A", Icon = "gift", IsSystem = true },
        new() { Name = "Transport", Type = "Expense", ColorHex = "#3B82F6", Icon = "car", IsSystem = true },
        new() { Name = "Sănătate", Type = "Expense", ColorHex = "#EF4444", Icon = "health", IsSystem = true },
        new() { Name = "Abonamente", Type = "Expense", ColorHex = "#8B5CF6", Icon = "subscription", IsSystem = true },
        new() { Name = "Shopping", Type = "Expense", ColorHex = "#EC4899", Icon = "shopping", IsSystem = true },
        new() { Name = "Educație", Type = "Expense", ColorHex = "#6366F1", Icon = "book", IsSystem = true },
        new() { Name = "Divertisment", Type = "Expense", ColorHex = "#F59E0B", Icon = "game", IsSystem = true },
        new() { Name = "Investiții", Type = "Expense", ColorHex = "#0F766E", Icon = "chart", IsSystem = true },
        new() { Name = "Transferuri", Type = "Both", ColorHex = "#64748B", Icon = "transfer", IsSystem = true },
        new() { Name = "Retrageri numerar", Type = "Expense", ColorHex = "#78716C", Icon = "cash", IsSystem = true },
        new() { Name = "Venituri", Type = "Income", ColorHex = "#16A34A", Icon = "income", IsSystem = true },
        new() { Name = "Altele", Type = "Both", ColorHex = "#9CA3AF", Icon = "other", IsSystem = true }
    };

        var existingCategories = await _database.Db.Table<CategoryEntity>().ToListAsync();

        foreach (var category in defaults)
        {
            var existing = existingCategories.FirstOrDefault(x =>
                x.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                await _database.Db.InsertAsync(category);
            }
            else if (existing.IsDeleted)
            {
                // important: nu readucem automat categoriile șterse de user
                continue;
            }
            else
            {
                existing.IsSystem = true;
                existing.Type = category.Type;
                existing.ColorHex = category.ColorHex;
                existing.Icon = category.Icon;
                await _database.Db.UpdateAsync(existing);
            }
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
            new() { Keyword = "AUCHAN", CategoryId = CategoryId("Supermarket"), },
            new() { Keyword = "LIDL", CategoryId = CategoryId("Supermarket"), Bank = "" },
            new() { Keyword = "PROFI", CategoryId = CategoryId("Supermarket"), Bank = "" },
            new() { Keyword = "MEGA IMAGE", CategoryId = CategoryId("Supermarket"), Bank = "" },
            new() { Keyword = "CARREFOUR", CategoryId = CategoryId("Supermarket"), Bank = "" },
            new() { Keyword = "KAUFLAND", CategoryId = CategoryId("Supermarket"), Bank = "" },
            new() { Keyword = "TERESY", CategoryId = CategoryId("Supermarket"), Bank = "" },

            // Restaurante / cafenele
            new() { Keyword = "SISTERS CAFE", CategoryId = CategoryId("Restaurante / Cafenele"), Bank = "" },
            new() { Keyword = "INM CJ IULIUS", CategoryId = CategoryId("Restaurante / Cafenele"), Bank = "" },
            new() { Keyword = "MCDONALD", CategoryId = CategoryId("Restaurante / Cafenele"), Bank = "" },
            new() { Keyword = "KFC", CategoryId = CategoryId("Restaurante / Cafenele"), Bank = "" },
            new() { Keyword = "STARBUCKS", CategoryId = CategoryId("Restaurante / Cafenele"), Bank = "" },

            // Transport
            new() { Keyword = "BOLT", CategoryId = CategoryId("Transport"), Bank = "" },
            new() { Keyword = "UBER", CategoryId = CategoryId("Transport") },
            new() { Keyword = "BILETERIA", CategoryId = CategoryId("Transport"), Bank = "" },
            new() { Keyword = "AUTOGARI", CategoryId = CategoryId("Transport"), Bank = "" },

            // Sănătate
            new() { Keyword = "SYNEVO", CategoryId = CategoryId("Sănătate"), Bank = "" },
            new() { Keyword = "REMEDIUM", CategoryId = CategoryId("Sănătate"), Bank = "" },
            new() { Keyword = "FARM", CategoryId = CategoryId("Sănătate"), Bank = "" },
            new() { Keyword = "PHARM", CategoryId = CategoryId("Sănătate"), Bank = "" },

            // Abonamente
            new() { Keyword = "SPOTIFY", CategoryId = CategoryId("Abonamente"), Bank = "" },
            new() { Keyword = "NETFLIX", CategoryId = CategoryId("Abonamente"), Bank = "" },
            new() { Keyword = "OPENAI", CategoryId = CategoryId("Abonamente"), Bank = "" },
            new() { Keyword = "CHATGPT", CategoryId = CategoryId("Abonamente"), Bank = ""},
            new() { Keyword = "GEOGUESSR", CategoryId = CategoryId("Abonamente"), Bank = "" },

            // Shopping
            new() { Keyword = "EMAG", CategoryId = CategoryId("Shopping"), Bank = "" },
            new() { Keyword = "FASHION", CategoryId = CategoryId("Shopping"), Bank = "" },
            new() { Keyword = "MI.COM", CategoryId = CategoryId("Shopping"), Bank = "" },

            // Divertisment
            new() { Keyword = "SUPERCELL", CategoryId = CategoryId("Divertisment"), Bank = "" },

            // Investiții
            new() { Keyword = "BT OBLIGATIUNI", CategoryId = CategoryId("Investiții"), Bank = "" },
            new() { Keyword = "BT INDEX ROMANIA", CategoryId = CategoryId("Investiții"), Bank = "" },
            new() { Keyword = "ACHIZITIE UNITATI DE FOND", CategoryId = CategoryId("Investiții"), Bank = "" },

            // Transferuri
            new() { Keyword = "TRANSFER INTRE CONTURILE PROPRII", CategoryId = CategoryId("Transferuri"), Bank = "" },
            new() { Keyword = "TRANSFER BT PAY", CategoryId = CategoryId("Transferuri"), Bank = "" },
            new() { Keyword = "P2P BTPAY", CategoryId = CategoryId("Transferuri"), Bank = "" },
            new() { Keyword = "PLATA INSTANT", CategoryId = CategoryId("Transferuri"), Bank = "" },

            // Retrageri / numerar
            new() { Keyword = "RETRAGERE NUMERAR", CategoryId = CategoryId("Retrageri numerar"), Bank = "" },
            new() { Keyword = "RETRAGERI DE NUMERAR", CategoryId = CategoryId("Retrageri numerar"), Bank = "" },

            // Venituri
            new() { Keyword = "TRANSFER CREDIT", CategoryId = CategoryId("Venituri"), Bank = "" },
            new() { Keyword = "INCASARE", CategoryId = CategoryId("Venituri"), Bank = "" },
            new() { Keyword = "DEPUNERE NUMERAR", CategoryId = CategoryId("Venituri"), Bank = "" },
        };

        foreach (var rule in rules)
            await _database.Db.InsertAsync(rule);
    }

    public async Task<List<CategoryEntity>> GetCategoriesAsync()
    {
        return await _database.Db
            .Table<CategoryEntity>()
            .Where(x => !x.IsDeleted)
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
                searchableText.Contains(rule.Keyword.ToUpperInvariant()) &&
                rule.Bank.Equals(transaction.Bank, StringComparison.OrdinalIgnoreCase))
            ??
            rules.FirstOrDefault(rule =>
                searchableText.Contains(rule.Keyword.ToUpperInvariant()) &&
                string.IsNullOrWhiteSpace(rule.Bank));

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

    public async Task AddMerchantRuleAsync(string keyword, int categoryId, string bank)
    {
        keyword = keyword.Trim().ToUpperInvariant();
        bank = bank.Trim();

        if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(bank))
            return;

        var existing = await _database.Db.Table<MerchantRuleEntity>()
            .FirstOrDefaultAsync(x =>
                x.Keyword.ToUpper() == keyword &&
                x.Bank.ToUpper() == bank.ToUpper());

        if (existing != null)
            return;

        await _database.Db.InsertAsync(new MerchantRuleEntity
        {
            Keyword = keyword,
            CategoryId = categoryId,
            Bank = bank
        });
    }
    public async Task AddCategoryAsync(string name, string type, string colorHex = "#3F51B5")
    {
        name = name.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        var existing = await _database.Db.Table<CategoryEntity>()
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.Type = type;
                existing.ColorHex = colorHex;

                await _database.Db.UpdateAsync(existing);
            }

            return;
        }

        await _database.Db.InsertAsync(new CategoryEntity
        {
            Name = name,
            Type = type,
            ColorHex = colorHex,
            Icon = "",
            IsSystem = false,
            IsDeleted = false
        });
    }
    public async Task DeleteCategoryAsync(int categoryId)
    {
        var category = await _database.Db.Table<CategoryEntity>()
            .FirstOrDefaultAsync(x => x.Id == categoryId);

        if (category == null)
            return;

        category.IsDeleted = true;
        await _database.Db.UpdateAsync(category);

        var rules = await _database.Db.Table<MerchantRuleEntity>()
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();

        foreach (var rule in rules)
            await _database.Db.DeleteAsync(rule);
    }
    public async Task<bool> IsCategoryUsedAsync(int categoryId)
    {
        var count = await _database.Db.Table<TransactionEntity>()
            .Where(x => x.CategoryId == categoryId)
            .CountAsync();

        return count > 0;
    }
}