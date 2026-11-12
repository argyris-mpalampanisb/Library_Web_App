namespace Library_Web_App.Models;

public class Rental
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public DateTime RentalDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}