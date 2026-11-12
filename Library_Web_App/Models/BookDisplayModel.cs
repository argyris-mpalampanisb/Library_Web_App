namespace Library_Web_App.Models;

public class BookDisplayModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool InCart { get; set; }
    public bool OnLoan { get; set; }
}