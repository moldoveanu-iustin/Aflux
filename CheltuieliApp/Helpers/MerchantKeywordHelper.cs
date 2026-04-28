using System.Text.RegularExpressions;

namespace CheltuieliApp.Helpers;

public static class MerchantKeywordHelper
{
    public static string SuggestKeyword(string merchant)
    {
        if (string.IsNullOrWhiteSpace(merchant))
            return "";

        var value = merchant.Trim().ToUpperInvariant();

        // BRD: OP xxxx Card nr....1234 AUCHAN...
        var brdMatch = Regex.Match(
            value,
            @"CARD\s+NR\.{2,}\d+\s*(?<merchant>.+)$",
            RegexOptions.IgnoreCase);

        if (brdMatch.Success)
            return ExtractRelevantKeyword(brdMatch.Groups["merchant"].Value);

        return ExtractRelevantKeyword(value);
    }

    private static string ExtractRelevantKeyword(string value)
    {
        value = value
            .Trim()
            .ToUpperInvariant()
            .Replace("*", " ")
            .Replace(";", " ")
            .Replace(",", " ")
            .Replace(".", "."); // păstrăm MI.COM / PADDLE.NET

        var tokens = value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim(':', '-', '/', '\\', '(', ')', '[', ']'))
            .Where(x => x.Length >= 2)
            .Where(x => !StopWords.Contains(x))
            .Where(x => !Regex.IsMatch(x, @"^\d+$"))
            .Where(x => !Regex.IsMatch(x, @"^\d{2,}_?\d*$"))
            .ToList();

        return tokens.FirstOrDefault() ?? "";
    }

    private static readonly HashSet<string> StopWords = new()
    {
        "OP", "CARD", "NR", "NUMAR", "NUMĂR",
        "POS", "EPOS", "TID", "MID", "RRN", "PAN",
        "RON", "EUR", "USD", "GBP",
        "RO", "ROM", "ROU",
        "TRANSFER", "PLATA", "PLATĂ", "INCASARE", "ÎNCASARE",
        "DEPUNERE", "NUMERAR", "RETRAGERE",
        "COMISION", "VALOARE", "TRANZACTIE", "TRANZACȚIE"
    };
}