using System.ComponentModel.DataAnnotations;

namespace Library_Web_App.Models;

public class RegisterViewModel
{
    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(100), MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(100), Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}