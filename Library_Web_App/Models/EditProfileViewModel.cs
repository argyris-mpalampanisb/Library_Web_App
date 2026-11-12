using System.ComponentModel.DataAnnotations;

namespace Library_Web_App.Models;

public class EditProfileViewModel
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string? NewPassword { get; set; }

    [StringLength(100), Compare("NewPassword")]
    public string? ConfirmNewPassword { get; set; }
}