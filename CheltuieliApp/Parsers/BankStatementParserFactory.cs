using CheltuieliApp.DTOs;

namespace CheltuieliApp.Parsers;

public class BankStatementParserFactory
{
    private readonly List<IBankStatementParser> _parsers =
    [
        new BtStatementParser(),
        new BrdStatementParser(),
        new RaiffeisenStatementParser()

    ];

    public BankStatementDto Parse(string text)
    {
        var parser = _parsers.FirstOrDefault(x => x.CanParse(text));

        if (parser == null)
            throw new Exception("Nu am putut identifica banca extrasului.");

        return parser.Parse(text);
    }
}