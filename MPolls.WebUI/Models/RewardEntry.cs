namespace MPolls.WebUI.Models;

public class RewardEntry
{
    public string Description { get; set; }
    public DateTime CompletedOn { get; set; }
    public int Points { get; set; }
    public bool IsExpired { get; set; }
}