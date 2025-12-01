using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Models;
using MPolls.WebUI.Models.Survey;
using MPolls.WebUI.Services;

namespace MPolls.WebUI.Pages;

public partial class ProfileCategoryDetails : ComponentBase
{
    [Parameter]
    public int CategoryId { get; set; }

    [Inject]
    private SurveyCategoriesClient SurveyCategoriesClient { get; set; } = default!;

    [Inject]
    private ProfileSurveyClient ProfileSurveyClient { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AuthState AuthState { get; set; } = default!;

    [Inject]
    private PanelistClient PanelistClient { get; set; } = default!;

    private SurveyCategoryModel? _category;
    private ProfileSurveyDetailsModel? _details;
    private bool _isLoading = true;
    private string? _errorMessage;
    private string? _panelistUlid;

    private bool ShowRewardGuidance => _category is not null;

    private string RewardPrimaryMessage
    {
        get
        {
            if (_category is null)
            {
                return string.Empty;
            }

            if (!HasResponses)
            {
                return $"Complete this Profile to earn {FormatPoints(_category.RewardPoints)} reward points.";
            }

            var formattedRetakePoints = FormatPoints(_category.RetakePoints);

            if (IsRetakeAvailableNow)
            {
                return $"You have already completed the Profile. You can retake the survey now for {formattedRetakePoints} reward points.";
            }

            var waitText = FormatRetakeWaitText();
            return $"You have already completed the Profile. You can retake it in {waitText} for {formattedRetakePoints} reward points.";
        }
    }

    private string RewardChipText
    {
        get
        {
            if (_category is null)
            {
                return string.Empty;
            }

            var points = !HasResponses ? _category.RewardPoints : _category.RetakePoints;
            return FormatPoints(points);
        }
    }

    private string ActionButtonText => HasResponses ? "Retake" : "Start";

    private bool IsActionDisabled => HasResponses && !IsRetakeAvailableNow;

    private string ActionSupportingText
    {
        get
        {
            if (!HasQuestions)
            {
                return string.Empty;
            }

            if (!HasResponses)
            {
                return "Start now to unlock more tailored opportunities.";
            }

            return IsRetakeAvailableNow
                ? "Refresh your answers to keep your opportunities relevant."
                : "We'll let you know once this profile is ready for another update.";
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await AuthState.InitializeAsync();

        if (!AuthState.IsAuthenticated)
        {
            NavigationManager.NavigateTo("/error/403Forbidden", forceLoad: false, replace: true);
            return;
        }

        await EnsurePanelistUlidAsync();

        try
        {
            var categories = await SurveyCategoriesClient.GetSurveyCategoriesAsync();
            _category = categories.FirstOrDefault(category => category.CategoryId == CategoryId);

            if (_category is null)
            {
                _errorMessage = "We couldn't find that profile category.";
                return;
            }

            _details = await ProfileSurveyClient.GetProfileSurveyDetailsAsync(CategoryId);

            if (_details is null)
            {
                _errorMessage = "We couldn't load your profile details right now. Please try again later.";
            }
        }
        catch
        {
            _errorMessage = "We couldn't load your profile details right now. Please try again later.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private bool HasQuestions => (_details?.TotalQuestionCount ?? 0) > 0;

    private bool HasResponses => _details?.Responses?.Count > 0;

    private bool IsRetakeAvailableNow
    {
        get
        {
            if (!HasResponses || _category is null)
            {
                return false;
            }

            if (_category.RetakePointsIssueFrequency <= 0)
            {
                return true;
            }

            var lastResponse = _details?.LastResponseOn;

            if (!lastResponse.HasValue)
            {
                return true;
            }

            var normalizedLastResponse = EnsureUtc(lastResponse.Value);
            var nextEligible = normalizedLastResponse.AddDays(_category.RetakePointsIssueFrequency);

            return DateTime.UtcNow >= nextEligible;
        }
    }

    private bool CanShowResponses => !_isLoading && string.IsNullOrWhiteSpace(_errorMessage) && HasResponses;

    private IReadOnlyList<ProfileQuestionDetailModel> Responses => _details?.Responses ?? Array.Empty<ProfileQuestionDetailModel>();

    private static IReadOnlyList<ProfileAnswerDetailModel> GetAnswers(ProfileQuestionDetailModel question) =>
        question.Answers ?? Array.Empty<ProfileAnswerDetailModel>();

    private string? LastUpdatedOnText => FormatLastUpdatedOn(_details?.LastResponseOn);

    private void StartOrRetakeSurvey()
    {
        var panelistUlid = string.IsNullOrWhiteSpace(_panelistUlid)
            ? AuthState.CurrentUser?.Ulid
            : _panelistUlid;

        if (string.IsNullOrWhiteSpace(panelistUlid))
        {
            NavigationManager.NavigateTo("/error/403Forbidden", replace: true);
            return;
        }

        var context = new SurveyNavigationContext(CategoryId, panelistUlid.Trim());
        var encoded = SurveyNavigationContext.Encode(context);
        var escaped = Uri.EscapeDataString(encoded);

        NavigationManager.NavigateTo($"/engage/profile?c={escaped}");
    }

    private async Task EnsurePanelistUlidAsync()
    {
        var currentUser = AuthState.CurrentUser;

        if (currentUser is null)
        {
            _panelistUlid = null;
            return;
        }

        if (!string.IsNullOrWhiteSpace(currentUser.Ulid))
        {
            _panelistUlid = currentUser.Ulid.Trim();
        }

        var resolvedUlid = await PanelistClient.GetPanelistUlidAsync();

        if (string.IsNullOrWhiteSpace(resolvedUlid))
        {
            return;
        }

        _panelistUlid = resolvedUlid.Trim();
        await AuthState.UpdateUlidAsync(_panelistUlid);
    }

    private string FormatRetakeWaitText()
    {
        if (_category is null)
        {
            return "a few days";
        }

        var frequency = _category.RetakePointsIssueFrequency;

        if (frequency <= 0)
        {
            return "a few days";
        }

        var nextEligible = GetNextRetakeEligibleOn();

        if (!nextEligible.HasValue)
        {
            return FormatDayCount(frequency);
        }

        var normalizedNextEligible = EnsureUtc(nextEligible.Value);
        var now = DateTime.UtcNow;

        if (now >= normalizedNextEligible)
        {
            return FormatDayCount(frequency);
        }

        var remaining = normalizedNextEligible - now;
        var remainingDays = (int)Math.Ceiling(remaining.TotalDays);
        remainingDays = remainingDays <= 0 ? 1 : remainingDays;

        return FormatDayCount(remainingDays);
    }

    private DateTime? GetNextRetakeEligibleOn()
    {
        if (!HasResponses || _category is null)
        {
            return null;
        }

        if (_category.RetakePointsIssueFrequency <= 0)
        {
            return _details?.LastResponseOn;
        }

        var lastResponse = _details?.LastResponseOn;

        if (!lastResponse.HasValue)
        {
            return null;
        }

        var normalizedLastResponse = EnsureUtc(lastResponse.Value);
        return normalizedLastResponse.AddDays(_category.RetakePointsIssueFrequency);
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => value
    };

    private static string FormatDayCount(int days)
    {
        if (days <= 0)
        {
            return "a few days";
        }

        if (days % 30 == 0)
        {
            var months = days / 30;
            return months == 1
                ? "30 days (about 1 month)"
                : $"{days} days (about {months} months)";
        }

        if (days % 7 == 0)
        {
            var weeks = days / 7;
            return weeks == 1
                ? "7 days (1 week)"
                : $"{days} days (about {weeks} weeks)";
        }

        return days == 1 ? "1 day" : $"{days} days";
    }

    private static string FormatPoints(int points)
    {
        var safePoints = points < 0 ? 0 : points;
        return safePoints.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static string? FormatLastUpdatedOn(DateTime? lastResponseOn)
    {
        if (!lastResponseOn.HasValue)
        {
            return null;
        }

        return lastResponseOn.Value.ToString("MMMM d, yyyy 'at' h:mm tt", CultureInfo.CurrentCulture);
    }
}
