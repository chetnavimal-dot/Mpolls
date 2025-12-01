using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Models;
using MPolls.WebUI.Models.Auth;
using MPolls.WebUI.Services;
using MudBlazor;

namespace MPolls.WebUI.Pages;

public partial class Dashboard : ComponentBase
{
    [Inject]
    private AuthState AuthState { get; set; } = default!;
    
    [Inject]
    private DashboardClient DashboardClient { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool _isLoading;
    private DashboardOverview _overview;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        // Ensure the shared auth state hydrates from the server before evaluating permissions.
        await AuthState.InitializeAsync();

        if (!AuthState.IsAuthenticated)
        {
            // Redirect unauthenticated visitors to the dedicated 403 surface for consistent recovery guidance.
            _isLoading = false;
            NavigationManager.NavigateTo("/error/403Forbidden", forceLoad: false, replace: true);
            return;
        }
        
        if (!AuthState.CurrentUser.IsOnboarded)
        {
            NavigationManager.NavigateTo("/onboarding");
        }
        
        var dashBoardDetails = await DashboardClient.GetDashboardDetails();
        _overview = DashboardOverview.CreateDefault(AuthState.CurrentUser, dashBoardDetails);

        _isLoading = false;
    }

    private sealed record DashboardOverview(
        string GreetingName,
        IReadOnlyList<DashboardStat> Stats,
        IReadOnlyList<DashboardSurveySummary> UpcomingSurveys,
        IReadOnlyList<DashboardActionItem> NextSteps)
    {
        public static DashboardOverview CreateDefault(AuthLoginResponse? user, DashboardResponse? response)
        {
            var displayName = string.IsNullOrWhiteSpace(user?.DisplayName)
                ? user?.Email ?? "there"
                : user.DisplayName;

            var stats = new List<DashboardStat>
            {
                new("Active surveys", $"{response.ActiveSurveys.Count}", Icons.Material.Filled.Ballot, Color.Primary, "Opportunities waiting for your feedback."),
                new("Reward balance", $"{response.RewardBalance:N0} pts", Icons.Material.Filled.CardGiftcard, Color.Secondary, "Ready to redeem in the rewards catalog."),
                new("Profile strength", $"{response.ProfileStrength}%", Icons.Material.Filled.VerifiedUser, Color.Success, "Complete your profile to unlock more matches."),
                new("Responses this month", $"{response.MonthlySurveys} surveys", Icons.Material.Filled.Insights, Color.Info, "Keep sharing insights to grow your rewards.")
            };

            var surveys = new List<DashboardSurveySummary>();

            for (int i = 0; i < response.ActiveSurveys.Count &&  i < 3; i++)
            {
                var now = DateTime.UtcNow;
                var diff = response.ActiveSurveys[i].ExpiringOn.Value.Date - now.Date;
                surveys.Add(new(response.ActiveSurveys[i].SurveyName, $"Closes in {diff.Days} days", $"Earn {response.ActiveSurveys[i].EstimatedRewardPoints} pts", Icons.Material.Filled.Note));
            }

            var actions = new List<DashboardActionItem>
            {
                new("Review your profile interests", "Refresh your category preferences so we can match you to the best surveys.", Icons.Material.Filled.ManageAccounts),
                new("Redeem available rewards", "You have points ready to redeem—treat yourself in the rewards section.", Icons.Material.Filled.CardGiftcard),
                new("Check new surveys", "Visit the surveys page to take the latest studies matched to you.", Icons.Material.Filled.NotificationsActive)
            };

            if (user is not null && !user.vFlag)
            {
                actions.Insert(0, new DashboardActionItem(
                    "Verify your email address",
                    $"Confirm the link we sent to {user.Email} to start redeeming rewards faster.",
                    Icons.Material.Filled.MarkEmailUnread));
            }

            return new DashboardOverview(displayName, stats, surveys, actions);
        }
    }

    private sealed record DashboardStat(string Label, string Value, string Icon, Color Color, string Description);

    private sealed record DashboardSurveySummary(string Title, string Details, string Reward, string Icon);

    private sealed record DashboardActionItem(string Title, string Description, string Icon);
}
