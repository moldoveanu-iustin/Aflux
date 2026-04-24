using CheltuieliApp.DTOs;

namespace CheltuieliApp.Parsers;

public interface IBankStatementParser
{
    bool CanParse(string text);
    BankStatementDto Parse(string text);
}