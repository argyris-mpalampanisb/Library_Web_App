namespace Library_Web_App.Models;

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}