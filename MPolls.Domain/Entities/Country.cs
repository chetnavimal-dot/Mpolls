namespace MPolls.Domain.Entities;

public class Country
{
    public string CountryName { get; set; } = string.Empty;

    public string CountryShortCode { get; set; } = string.Empty;

    public int CountryCode { get; set; }

    public bool IsActive { get; set; }
}
