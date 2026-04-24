using CheltuieliApp.DTOs;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CheltuieliApp.Parsers;

public class BtStatementParser : IBankStatementParser
{
    public bool CanParse(string text)
    {
        return text.Contains("BANCA TRANSILVANIAInfo clienti", StringComparison.OrdinalIgnoreCase)
            || text.Contains("EXTRAS CONTNumarul:", StringComparison.OrdinalIgnoreCase)
            && text.Contains("Cod IBAN: RO", StringComparison.OrdinalIgnoreCase)
            && text.Contains("BTRL", StringComparison.OrdinalIgnoreCase);
    }

    public BankStatementDto Parse(string text)
    {
        text = FixPdfText(text);

        var statement = new BankStatementDto
        {
            Bank = "BT",
            AccountIban = ExtractIban(text)
        };

        var dates = Regex.Matches(text, @"\b\d{2}/\d{2}/\d{4}\b")
            .Select(x => DateTime.ParseExact(x.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        if (dates.Any())
        {
            statement.PeriodStart = dates.First();
            statement.PeriodEnd = dates.Last();
        }

        statement.Transactions = ExtractTransactions(text, statement.AccountIban);

        return statement;
    }
    private static string FixPdfText(string text)
    {
        text = text.Replace("IncasareInstant", "Incasare Instant");
        text = text.Replace("P2PBTPay", "P2P BTPay");
        text = text.Replace("PlatalаPOS", "Plata la POS");
        text = text.Replace("DepunerenumerarATM", "Depunere numerar ATM");
        text = text.Replace("Achizitieunitatidefond", "Achizitie unitati de fond");

        text = text.Replace("DataDescriereDebitCredit", "Data Descriere Debit Credit");

        return text;
    }


    private static List<BankTransactionDto> ExtractTransactions(string text, string iban)
    {
        var result = new List<BankTransactionDto>();

        text = Normalize(text);

        var dayMatches = Regex.Matches(
            text,
            @"(?<date>\d{2}/\d{2}/\d{4})(?<content>.*?)(?<sameDate>\d{2}/\d{2}/\d{4})RULAJ ZI",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match dayMatch in dayMatches)
        {
            var dateText = dayMatch.Groups["date"].Value;

            var date = DateTime.ParseExact(
                dateText,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture);

            var content = dayMatch.Groups["content"].Value;

            content = Regex.Replace(
                content,
                @"^SOLD ANTERIOR[\d,.]+",
                "",
                RegexOptions.IgnoreCase);

            var rawTransactions = SplitBtTransactions(content);

            foreach (var raw in rawTransactions)
            {
                var transaction = ParseBtTransaction(raw, date, iban);

                if (transaction != null)
                    result.Add(transaction);
            }
        }

        return result;
    }
    private static List<string> SplitBtTransactions(string content)
    {
        var markers = new[]
        {
        "Incasare Instant",
        "P2P BTPay",
        "Plata la POS non-BT cu card MASTERCARD",
        "Plata la POS",
        "Depunere numerar ATM",
        "Achizitie unitati de fond"
    };

        var markerPattern = string.Join("|", markers.Select(Regex.Escape));

        var matches = Regex.Matches(
            content,
            $@"(?={markerPattern})",
            RegexOptions.IgnoreCase);

        var result = new List<string>();

        for (var i = 0; i < matches.Count; i++)
        {
            var start = matches[i].Index;
            var end = i + 1 < matches.Count
                ? matches[i + 1].Index
                : content.Length;

            var block = content[start..end].Trim();

            if (!string.IsNullOrWhiteSpace(block))
                result.Add(block);
        }

        return result;
    }
    private static BankTransactionDto? ParseBtTransaction(string raw, DateTime date, string iban)
    {
        raw = Normalize(raw);

        if (!raw.Contains("REF:", StringComparison.OrdinalIgnoreCase))
            return null;

        var amount = ExtractAmountAfterRef(raw);

        if (amount <= 0)
            return null;

        return new BankTransactionDto
        {
            Bank = "BT",
            AccountIban = iban,
            TransactionDate = date,
            Amount = amount,
            Direction = GetDirection(raw),
            Merchant = ExtractMerchant(raw),
            Description = raw,
            RawText = raw
        };
    }


    private static decimal ExtractAmountAfterRef(string raw)
    {
        var refIndex = raw.LastIndexOf("REF:", StringComparison.OrdinalIgnoreCase);

        if (refIndex < 0)
            return 0;

        var afterRef = raw[(refIndex + 4)..];

        var matches = Regex.Matches(afterRef, @"\b\d{1,3}(?:,\d{3})*\.\d{2}\b|\b\d+\.\d{2}\b");

        if (matches.Count == 0)
            return 0;

        var value = matches[^1].Value.Replace(",", "");

        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var amount)
            ? amount
            : 0;
    }

    private static string GetDirection(string raw)
    {
        if (raw.StartsWith("Incasare Instant", StringComparison.OrdinalIgnoreCase))
            return "Credit";

        if (raw.StartsWith("Depunere numerar ATM", StringComparison.OrdinalIgnoreCase))
            return "Credit";

        return "Debit";
    }

    private static string ExtractMerchant(string raw)
    {
        if (raw.StartsWith("Incasare Instant", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(raw, @"Transfer\s*-\s*(?<name>.*?);", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups["name"].Value.Trim();

            return "Transfer primit";
        }

        if (raw.StartsWith("P2P BTPay", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(raw, @"catre\s+(?<name>.*?)\s+reprezentand", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups["name"].Value.Trim();

            return "Transfer BT Pay";
        }

        if (raw.StartsWith("Achizitie unitati de fond", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(raw, @";\s*(?<name>.*?);?\s*REF:", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups["name"].Value.Trim();

            return "Achizitie unitati de fond";
        }

        if (raw.StartsWith("Depunere numerar ATM", StringComparison.OrdinalIgnoreCase))
            return "Depunere numerar ATM";

        if (raw.Contains("TID:", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(
                raw,
                @"TID:\s*[^ ]+\s+(?<merchant>.*?)(?:\s+valoare|\s+RRN:)",
                RegexOptions.IgnoreCase);

            if (match.Success)
                return CleanupMerchant(match.Groups["merchant"].Value);
        }

        if (raw.Contains("MID", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(
                raw,
                @"MID\s+[A-Z0-9]+\s+(?<merchant>.*?)(?:\s+valoare|\s+RRN:)",
                RegexOptions.IgnoreCase);

            if (match.Success)
                return CleanupMerchant(match.Groups["merchant"].Value);
        }

        return "Necunoscut";
    }

    private static string CleanupMerchant(string merchant)
    {
        merchant = Normalize(merchant);

        merchant = Regex.Replace(merchant, @"\bRON\b", "", RegexOptions.IgnoreCase);
        merchant = Regex.Replace(merchant, @"\bRO\b", "", RegexOptions.IgnoreCase);
        merchant = Regex.Replace(merchant, @"\bPT\b", "", RegexOptions.IgnoreCase);
        merchant = Regex.Replace(merchant, @"\bIE\b", "", RegexOptions.IgnoreCase);

        return merchant.Trim(' ', ';', ',');
    }

    private static string ExtractIban(string text)
    {
        var match = Regex.Match(
            text,
            @"RO\d{2}BTRL[A-Z0-9]{16}",
            RegexOptions.IgnoreCase);

        return match.Success ? match.Value.ToUpper() : "";
    }

    private static string Normalize(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}