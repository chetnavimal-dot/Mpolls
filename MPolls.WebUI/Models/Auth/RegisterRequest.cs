using System.ComponentModel.DataAnnotations;

namespace MPolls.WebUI.Models.Auth;

public class RegisterRequest : AuthRequest
{
    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
