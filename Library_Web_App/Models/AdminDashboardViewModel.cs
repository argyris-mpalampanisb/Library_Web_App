namespace Library_Web_App.Models;

public class AdminDashboardViewModel
{
    public List<RentalChartItem> ChartData { get; set; } = new();
    public int TotalUsers { get; set; }
    public int TotalBooks { get; set; }
    public int ActiveRentals { get; set; }
}

public class RentalChartItem
{
    public string BookTitle { get; set; } = string.Empty;
    public int DaysRented { get; set; }
    public int RentalCount { get; set; }
}