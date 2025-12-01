using System.ComponentModel.DataAnnotations;

namespace MPolls.WebUI.Models.Auth;

public class PasswordChangeRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
