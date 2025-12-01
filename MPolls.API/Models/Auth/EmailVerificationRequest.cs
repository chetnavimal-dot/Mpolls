using System.ComponentModel.DataAnnotations;

namespace MPolls.API.Models.Auth;

public class EmailVerificationRequest
{
    public string? IdToken { get; set; }
}
