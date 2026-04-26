namespace CheltuieliApp.DTOs;

public class DashboardDayDto
{
    public DateTime Date { get; set; }
    public int DayNumber => Date.Day;
    public bool IsCurrentMonth { get; set; }
    public bool IsToday => Date.Date == DateTime.Today;

    public bool HasBt { get; set; }
    public bool HasBrd { get; set; }
    public bool HasRaiffeisen { get; set; }
    public bool HasTransactions { get; set; }
}