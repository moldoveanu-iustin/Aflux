public class BankTransactionDto
{
    public string Bank { get; set; } = "";
    public string AccountIban { get; set; } = "";

    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }

    public string Direction { get; set; } = ""; // Debit / Credit
    public string Merchant { get; set; } = "";
    public string Description { get; set; } = "";

    public string RawText { get; set; } = "";
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
}