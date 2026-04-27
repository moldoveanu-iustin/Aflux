namespace CheltuieliApp.Helpers;

public static class MerchantKeywordHelper
{
    public static string SuggestKeyword(string merchant)
    {
        if (string.IsNullOrWhiteSpace(merchant))
            return "";

        var cleaned = merchant
            .Trim()
            .ToUpperInvariant()
            .Replace("*", " ")
            .Replace(";", " ")
            .Replace(",", " ");

        return cleaned
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "";
    }
}