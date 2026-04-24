using CheltuieliApp.DTOs;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CheltuieliApp.Parsers;

public class RaiffeisenStatementParser : IBankStatementParser
{
    public bool CanParse(string text)
    {
        return text.Contains("RAIFFEISEN BANK S.A.", StringComparison.OrdinalIgnoreCase)
            || text.Contains("RZBRROBU", StringComparison.OrdinalIgnoreCase);
    }

    public BankStatementDto Parse(string text)
    {
        text = Normalize(text);

        var statement = new BankStatementDto
        {
            Bank = "Raiffeisen",
            AccountIban = ExtractIban(text)
        };

        ExtractPeriod(text, statement);

        statement.Transactions = ExtractTransactions(text, statement.AccountIban);

        return statement;
    }

    private static List<BankTransactionDto> ExtractTransactions(string text, string iban)
    {
        var result = new List<BankTransactionDto>();

        text = Normalize(text);

        var starts = Regex.Matches(
            text,
            @"(?<bookingDate>\d{2}\.\d{2}\.\d{4})(?<transactionDate>\d{2}\.\d{2}\.\d{4})");

        for (var i = 0; i < starts.Count; i++)
        {
            var start = starts[i].Index;
            var end = i + 1 < starts.Count ? starts[i + 1].Index : text.Length;

            var block = Normalize(text[start..end]);

            block = Regex.Split(
                block,
                @"Soldul zilei:|Sold final|Soldul creditor|Pagina",
                RegexOptions.IgnoreCase)[0];

            var transaction = ParseTransactionBlock(block, iban);

            if (transaction != null)
                result.Add(transaction);
        }

        return result;
    }

    private static BankTransactionDto? ParseTransactionBlock(string block, string iban)
    {
        var match = Regex.Match(
            block,
            @"^(?<bookingDate>\d{2}\.\d{2}\.\d{4})(?<transactionDate>\d{2}\.\d{2}\.\d{4})(?<rest>.+)$",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            return null;

        var transactionDate = DateTime.ParseExact(
            match.Groups["transactionDate"].Value,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture);

        var rest = Normalize(match.Groups["rest"].Value);

        rest = Regex.Replace(rest, @"(?<date>\d{2}/\d{2}/\d{4})(?<amount>\d{1,3}(?:,\d{3})*\.\d{2})", "${date} ${amount}");

        var amountMatches = Regex.Matches(rest,@"(?<!\d)(\d{1,3}(?:,\d{3})*\.\d{2}|\d+\.\d{2})(?!\d)");

        if (amountMatches.Count == 0)
            return null;

        var amountMatch = amountMatches[^1];
        var amount = ParseAmount(amountMatch.Value);

        if (amount <= 0)
            return null;

        var description = Normalize(rest[..amountMatch.Index]);

        return new BankTransactionDto
        {
            Bank = "Raiffeisen",
            AccountIban = iban,
            TransactionDate = transactionDate,
            Amount = amount,
            Direction = DetectDirection(description),
            Merchant = ExtractMerchant(description),
            Description = description,
            RawText = block
        };
    }

    private static string DetectDirection(string description)
    {
        if (description.Contains("Depunere numerar", StringComparison.OrdinalIgnoreCase))
            return "Credit";

        if (description.Contains("Bonus", StringComparison.OrdinalIgnoreCase))
            return "Credit";

        return "Debit";
    }

    private static string ExtractMerchant(string description)
    {
        description = Normalize(description);

        if (description.Contains("Depunere numerar", StringComparison.OrdinalIgnoreCase))
            return "Depunere numerar";

        if (description.Contains("Bonus", StringComparison.OrdinalIgnoreCase))
            return "Bonus card";

        if (description.Contains("Transfer intre conturile proprii", StringComparison.OrdinalIgnoreCase))
            return "Transfer între conturi proprii";

        if (description.Contains("BANCA TRANSILVANIA", StringComparison.OrdinalIgnoreCase))
            return "BANCA TRANSILVANIA S.A.";

        var cardMatch = Regex.Match(
            description,
            @"^(?<merchant>.*?)Card nr\.",
            RegexOptions.IgnoreCase);

        if (cardMatch.Success)
            return cardMatch.Groups["merchant"].Value.Trim();

        return "Tranzacție Raiffeisen";
    }

    private static string ExtractIban(string text)
    {
        var match = Regex.Match(
            text,
            @"Cod IBAN:\s*(?<iban>RO\d{2}\s*RZBR(?:\s*[A-Z0-9]){16,})",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            return "";

        return Regex.Replace(match.Groups["iban"].Value, @"\s+", "").ToUpper();
    }

    private static void ExtractPeriod(string text, BankStatementDto statement)
    {
        var match = Regex.Match(
            text,
            @"Perioada:\s*de la\s*(?<start>\d{2}\.\d{2}\.\d{4})\s*la\s*(?<end>\d{2}\.\d{2}\.\d{4})",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            return;

        statement.PeriodStart = DateTime.ParseExact(match.Groups["start"].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        statement.PeriodEnd = DateTime.ParseExact(match.Groups["end"].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture);
    }

    private static decimal ParseAmount(string value)
    {
        value = value.Replace(",", "");

        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var amount)
            ? amount
            : 0;
    }

    private static string Normalize(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}