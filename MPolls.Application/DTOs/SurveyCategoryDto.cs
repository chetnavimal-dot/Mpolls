namespace MPolls.Application.DTOs;

public class SurveyCategoryDto
{
    public int CategoryId { get; set; }

    public bool IsActive { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public int RewardPoints { get; set; }

    public int RetakePoints { get; set; }

    public int RetakePointsIssueFrequency { get; set; }
}
