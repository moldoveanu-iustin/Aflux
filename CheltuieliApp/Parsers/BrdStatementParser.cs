using CheltuieliApp.DTOs;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CheltuieliApp.Parsers;

public class BrdStatementParser : IBankStatementParser
{
    public bool CanParse(string text)
    {
        return text.Contains("BRD-Groupe Societe Generale", StringComparison.OrdinalIgnoreCase) || text.Contains("BRDEROBU", StringComparison.OrdinalIgnoreCase);
    }

    public BankStatementDto Parse(string text)
    {
        text = Normalize(text);

        var statement = new BankStatementDto
        {
            Bank = "BRD",
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

        var pattern =
            @"(?<amount>\d{1,3}(?:\.\d{3})*,\d{2}|\d+,\d{2})" +
            @"(?<channel>Card/Terminale|Mobile Omnichannel|BRD Office)" +
            @"(?<content>.*?)" +
            @"(?<transactionDate>\d{2}/\d{2}/\d{4})" +
            @"(?<valueDate>\d{2}/\d{2}/\d{4})" +
            @"(?<operation>Utilizare POS comerciant alte BC|Utilizare POS comerciant BRD|Utilizare POS comerciant strain\.|Retrageri de Numerar-ATM/MBA BRD|Transfer credit- Incasare intrab|Transfer credit- Inc\. intrab ABB|Transfer credit-Plata interbTrez|Plata instant)" +
            @"(?=\d{1,3}(?:\.\d{3})*,\d{2}(?:Card/Terminale|Mobile Omnichannel|BRD Office)|BRD-Groupe|Total debit|Sold final|$)";

        var matches = Regex.Matches(
            text,
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var amount = ParseRomanianAmount(match.Groups["amount"].Value);
            if (amount <= 0)
                continue;

            var channel = match.Groups["channel"].Value.Trim();
            var content = Normalize(match.Groups["content"].Value);
            var operation = Normalize(match.Groups["operation"].Value);

            var date = DateTime.ParseExact(
                match.Groups["transactionDate"].Value,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture);

            var raw = Normalize($"{channel} {content} {match.Groups["transactionDate"].Value} {match.Groups["valueDate"].Value} {operation}");

            result.Add(new BankTransactionDto
            {
                Bank = "BRD",
                AccountIban = iban,
                TransactionDate = date,
                Amount = amount,
                Direction = GetDirection(operation),
                Merchant = ExtractMerchant(channel, content, operation),
                Description = raw,
                RawText = raw
            });
        }

        return result;
    }

    private static string GetDirection(string operation)
    {
        if (operation.Contains("Transfer credit", StringComparison.OrdinalIgnoreCase))
            return "Credit";

        return "Debit";
    }

    private static string ExtractMerchant(string channel, string content, string operation)
    {
        content = Normalize(content);

        if (channel.Equals("Card/Terminale", StringComparison.OrdinalIgnoreCase))
        {
            var afterCard = Regex.Replace(content, @"^OP\s+\S+\s+Card\s+nr\.+\d+\s*", "", RegexOptions.IgnoreCase);
            afterCard = Normalize(afterCard);

            if (!string.IsNullOrWhiteSpace(afterCard))
                return afterCard;
        }

        if (operation.Contains("Transfer credit", StringComparison.OrdinalIgnoreCase))
        {
            var lines = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (content.Contains("UNIVERSITATEA BABES BOLYAI", StringComparison.OrdinalIgnoreCase))
                return "UNIVERSITATEA BABES BOLYAI";

            if (content.Contains("ANA MOLDOVEANU", StringComparison.OrdinalIgnoreCase))
                return "ANA MOLDOVEANU";

            if (content.Contains("ALEX-CONSTANTIN MOLDOVEANU", StringComparison.OrdinalIgnoreCase))
                return "ALEX-CONSTANTIN MOLDOVEANU";

            return "Transfer primit";
        }

        if (operation.Contains("Plata instant", StringComparison.OrdinalIgnoreCase))
        {
            if (content.Contains("Moldoveanu Iustin BT", StringComparison.OrdinalIgnoreCase))
                return "Moldoveanu Iustin BT";

            return "Plata instant";
        }

        if (operation.Contains("Retrageri", StringComparison.OrdinalIgnoreCase))
            return "Retragere numerar ATM";

        return "Necunoscut";
    }

    private static string ExtractIban(string text)
    {
        var match = Regex.Match(
            text,
            @"IBAN\s*(RO\d{2}BRDE[A-Z0-9]{16})",
            RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value.ToUpper() : "";
    }

    private static void ExtractPeriod(string text, BankStatementDto statement)
    {
        var match = Regex.Match(
            text,
            @"De la / FromLa / To(?<start>\d{2}/\d{2}/\d{4})(?<end>\d{2}/\d{2}/\d{4})",
            RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            match = Regex.Match(
                text,
                @"De la / From\s*(?<start>\d{2}/\d{2}/\d{4})\s*La / To\s*(?<end>\d{2}/\d{2}/\d{4})",
                RegexOptions.IgnoreCase);
        }

        if (!match.Success)
            return;

        statement.PeriodStart = DateTime.ParseExact(match.Groups["start"].Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        statement.PeriodEnd = DateTime.ParseExact(match.Groups["end"].Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    private static decimal ParseRomanianAmount(string value)
    {
        value = value.Replace(".", "").Replace(",", ".");

        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var result)
            ? result
            : 0;
    }

    private static string Normalize(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}