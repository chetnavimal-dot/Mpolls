using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Models;
using MPolls.WebUI.Services;

namespace MPolls.WebUI.Pages;

public partial class Rewards : ComponentBase
{
    [Inject]
    private UserRewardClient UserRewardClient { get; set; } = default!;

    [Inject]
    private AuthState AuthState { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    public RewardResponse RewardDetails { get; private set; } = new()
    {
        RewardEntries = Array.Empty<RewardEntry>()
    };

    protected static bool IsExpired(RewardEntry entry) => entry.IsExpired;

    protected override async Task OnInitializedAsync()
    {
        // Rewards are part of the authenticated experience; wait for the session before querying balances.
        await AuthState.InitializeAsync();

        if (!AuthState.IsAuthenticated)
        {
            // Anonymous visitors get directed to the dedicated 403 page for next steps.
            NavigationManager.NavigateTo("/error/403Forbidden", replace: true);
            return;
        }
        
        if (!AuthState.CurrentUser.IsOnboarded)
        {
            NavigationManager.NavigateTo("/onboarding");
        }

        var rewardDetails = await UserRewardClient.GetUserRewardsAsync();

        if (rewardDetails is not null)
        {
            rewardDetails.RewardEntries ??= Array.Empty<RewardEntry>();
            RewardDetails = rewardDetails;
        }
    }
}
