using System.Globalization;
using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Models.Survey;
using MPolls.WebUI.Services;

namespace MPolls.WebUI.Pages;

public partial class Surveys : ComponentBase
{
    [Inject]
    private AuthState AuthState { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private RecommendedSurveyClient RecommendedSurveyClient { get; set; } = default!;

    private readonly List<RecommendedSurveyModel> _recommendedSurveys = new();
    private List<RecommendedSurveyModel> _expiringToday = new();
    private List<RecommendedSurveyModel> _popularThisMonth = new();
    private List<RecommendedSurveyModel> _additionalSurveys = new();
    private List<RecommendedSurveyModel> _expiredThisWeek = new();
    private bool _isLoading;
    private string? _errorMessage;

    protected IReadOnlyList<RecommendedSurveyModel> ExpiringToday => _expiringToday;

    protected IReadOnlyList<RecommendedSurveyModel> PopularThisMonth => _popularThisMonth;

    protected IReadOnlyList<RecommendedSurveyModel> AdditionalSurveys => _additionalSurveys;

    protected override async Task OnInitializedAsync()
    {
        // Make sure the survey list respects the latest authentication state before rendering.
        await AuthState.InitializeAsync();

        if (!AuthState.IsAuthenticated)
        {
            // Surveys are only available to signed-in panelists; point anonymous visitors to the dedicated 403 page for guidance.
            NavigationManager.NavigateTo("/error/403Forbidden", forceLoad: false, replace: true);
            return;
        }

        var currentUser = AuthState.CurrentUser;

        if (currentUser is null || !currentUser.IsOnboarded)
        {
            NavigationManager.NavigateTo("/onboarding");
            return;
        }

        await LoadSurveysAsync();
    }

    private async Task LoadSurveysAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            var surveys = await RecommendedSurveyClient.GetRecommendedSurveysAsync();

            _recommendedSurveys.Clear();
            _recommendedSurveys.AddRange(
                surveys
                    .Where(survey => survey is not null && !survey.IsCompleted)
                    .OrderBy(survey => survey.ExpiringOn ?? DateTime.MaxValue)
                    .ThenBy(survey => survey.SurveyName, StringComparer.OrdinalIgnoreCase));

            UpdateSurveySections();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            _errorMessage = "We couldn't load your recommended surveys right now. Please try again later.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static string GetRewardDisplay(RecommendedSurveyModel survey)
    {
        if (survey.EstimatedRewardPoints <= 0)
        {
            return "Reward varies";
        }

        return $"{survey.EstimatedRewardPoints.ToString("N0", CultureInfo.CurrentCulture)} pts";
    }

    private static string GetAssignedDisplay(RecommendedSurveyModel survey)
    {
        var assignedOnLocal = ConvertToLocal(survey.AssignedOn);
        return $"{assignedOnLocal:MMM d, yyyy}";
    }

    private static string GetExpirationDisplay(RecommendedSurveyModel survey)
    {
        if (!survey.ExpiringOn.HasValue)
        {
            return "No expiration date";
        }

        var expiration = ConvertToLocal(survey.ExpiringOn.Value);

        return $"{expiration:MMM d, yyyy}";
    }

    private void UpdateSurveySections()
    {
        var today = DateTime.Today;

        _expiringToday = _recommendedSurveys
            .Where(survey => IsExpiringToday(survey, today))
            .OrderBy(survey => ConvertToLocal(survey.ExpiringOn!.Value))
            .ThenBy(survey => survey.SurveyName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var assignedThisMonth = _recommendedSurveys
            .Where(survey => (survey.ExpiringOn == null || survey.ExpiringOn > today) && !IsExpiringToday(survey, today) && IsPopularThisMonth(survey, today))
            .OrderByDescending(survey => survey.EstimatedRewardPoints)
            .ThenBy(survey => survey.SurveyName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var excludedIds = new HashSet<Guid>(_expiringToday.Select(survey => survey.Id));
        foreach (var survey in assignedThisMonth)
        {
            excludedIds.Add(survey.Id);
        }

        _popularThisMonth = assignedThisMonth;

        _additionalSurveys = _recommendedSurveys
            .Where(survey => !excludedIds.Contains(survey.Id))
            .OrderBy(survey => survey.ExpiringOn.HasValue ? ConvertToLocal(survey.ExpiringOn.Value) : DateTime.MaxValue)
            .ThenBy(survey => survey.SurveyName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        
        var now = DateTime.UtcNow;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);
        
        var expiredThisWeek = _recommendedSurveys
            .Where(s => s.ExpiringOn != null && s.ExpiringOn < now && s.ExpiringOn >= startOfWeek)
            .ToList();

        _expiredThisWeek = expiredThisWeek;
    }

    private static bool IsExpiringToday(RecommendedSurveyModel survey, DateTime today)
    {
        if (!survey.ExpiringOn.HasValue)
        {
            return false;
        }

        return ConvertToLocal(survey.ExpiringOn.Value).Date == today;
    }

    private static bool IsPopularThisMonth(RecommendedSurveyModel survey, DateTime today)
    {
        var assignedOn = ConvertToLocal(survey.AssignedOn);
        return assignedOn.Month == today.Month && assignedOn.Year == today.Year;
    }

    private static DateTime ConvertToLocal(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Local => dateTime,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime(),
            _ => dateTime.ToLocalTime()
        };
    }
}
