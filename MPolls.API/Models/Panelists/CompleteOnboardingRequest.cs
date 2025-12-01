using System.ComponentModel.DataAnnotations;

namespace MPolls.API.Models.Panelists;

public sealed class CompleteOnboardingRequest
{
    [Range(1, 120)]
    public int Age { get; set; }

    [Range(1, int.MaxValue)]
    public int Gender { get; set; }

    [Range(1, int.MaxValue)]
    public int CountryCode { get; set; }
}
