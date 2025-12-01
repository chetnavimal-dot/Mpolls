namespace MPolls.Application.DTOs;

public class CountryDto
{
    public int CountryCode { get; set; }

    public string CountryShortCode { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;
}
