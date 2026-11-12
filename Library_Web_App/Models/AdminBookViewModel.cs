using System.ComponentModel.DataAnnotations;

namespace Library_Web_App.Models;

public class AdminBookViewModel
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Author { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}