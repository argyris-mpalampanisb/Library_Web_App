using System.ComponentModel.DataAnnotations;

namespace Library_Web_App.Models;

public class LoginViewModel
{
    [Required, StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;
}