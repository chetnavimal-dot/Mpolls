namespace MPolls.WebUI.Models;

public class RewardResponse
{
    public int TotalRewardEarned { get; set; }
    public int TotalRewardRedeemed { get; set; }
    public int TotalRewardAvailable { get; set; }
    public int TotalRewardExpired { get; set; }
    public int TotalRewardClaimed { get; set; }
    public RewardEntry[]? RewardEntries { get; set; }
}