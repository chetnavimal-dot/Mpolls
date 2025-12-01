using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using MPolls.WebUI.Models;
using MPolls.WebUI.Models.Survey;
using MPolls.WebUI.Services;

namespace MPolls.WebUI.Pages;

public partial class Engage : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private ProfileSurveyClient ProfileSurveyClient { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AuthState AuthState { get; set; } = default!;

    [Inject]
    private SurveyCategoriesClient SurveyCategoriesClient { get; set; } = default!;

    [Inject]
    private SurveyResultsClient SurveyResultsClient { get; set; } = default!;

    private bool ShowForm;
    private bool _isLoading = true;
    private string? _loadErrorMessage;
    private SurveyJsSurveyModel? _surveyDefinition;
    private DotNetObjectReference<Engage>? _dotNetReference;
    private SurveyCategoryModel? _category;
    private SurveyNavigationContext? _navigationContext;
    private bool _hasAcknowledgedGuidelines;
    private bool _hasStartedSurvey;
    private bool _isReturningUser;
    private bool _surveyRendered;
    private bool _surveyCompleted;
    private string? _formattedResponsesJson;
    private EngageRewardResponseModel? _rewardResponse;

    private bool ShowCompletionSummary => _surveyCompleted;
    private bool ShouldShowSurveyContainer => ShowForm && _hasStartedSurvey && !_surveyCompleted;
    private bool HasAcknowledgedGuidelines
    {
        get => _hasAcknowledgedGuidelines;
        set
        {
            if (_hasStartedSurvey || _hasAcknowledgedGuidelines == value)
            {
                return;
            }

            _hasAcknowledgedGuidelines = value;
        }
    }
    private bool HasStartedSurvey => _hasStartedSurvey;
    private bool IsLoading => _isLoading;
    private string? LoadErrorMessage => _loadErrorMessage;
    private bool CanContinue => _hasAcknowledgedGuidelines && !_hasStartedSurvey;
    private bool CanShowRewardSummary => _category is not null;
    private bool ShowCompletionRewardSummary => _rewardResponse is not null;
    private bool HasResponses => !string.IsNullOrWhiteSpace(_formattedResponsesJson);
    private string FormattedResponsesJson => _formattedResponsesJson ?? string.Empty;

    private string ExpectedRewardPointsText
    {
        get
        {
            var points = GetExpectedRewardPoints();
            return FormatPoints(points);
        }
    }

    private string ExpectedRewardPointsLabel => _isReturningUser ? "Retake Points" : "Reward Points";

    private string CompletionRewardPointsText => FormatPoints(AwardedPoints);

    private string CompletionRewardLabel => AwardedPoints > 0 ? "Points Collected" : "No Points Awarded";

    private string CompletionTitle => "Survey Complete";

    private string CompletionMessage
    {
        get
        {
            if (_category is null || string.IsNullOrWhiteSpace(_category.CategoryName))
            {
                return _isReturningUser
                    ? "Thanks for updating your profile."
                    : "Thanks for completing your profile.";
            }

            var categoryName = _category.CategoryName.Trim();

            return _isReturningUser
                ? $"Thanks for updating your {categoryName} profile."
                : $"Thanks for completing your {categoryName} profile.";
        }
    }

    private string CompletionRewardDescription
    {
        get
        {
            if (_category is null)
            {
                return "Your responses have been recorded.";
            }

            var points = AwardedPoints;
            var formattedPoints = FormatPoints(points);

            if (points <= 0)
            {
                return "Thanks for sharing your responses. Your rewards balance is unchanged for this survey.";
            }

            return _isReturningUser
                ? $"We've added {formattedPoints} retake points to your balance."
                : $"We've added {formattedPoints} reward points to your balance.";
        }
    }

    private string SurveyIntroTitle
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_category?.CategoryName))
            {
                return "Profile Survey";
            }

            return $"{_category.CategoryName.Trim()} Profile Survey";
        }
    }

    private string SurveyOverviewText
    {
        get
        {
            var categoryName = string.IsNullOrWhiteSpace(_category?.CategoryName)
                ? null
                : _category.CategoryName.Trim();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return "This is a profile survey. Please review the guidelines before you begin.";
            }

            return $"This is a profile survey for category {categoryName}. Please review the guidelines before you begin.";
        }
    }

    private string RewardSummaryText
    {
        get
        {
            if (_category is null)
            {
                return "Complete this profile survey to earn rewards for your account.";
            }

            var points = FormatPoints(GetExpectedRewardPoints());

            return _isReturningUser
                ? $"You're updating an existing profile. Completing this survey will add {points} retake points to your balance."
                : $"You're completing this profile for the first time. Completing this survey will add {points} reward points to your balance.";
        }
    }

    private int AwardedPoints => _rewardResponse?.PointsCollected ?? 0;

    protected override async Task OnInitializedAsync()
    {
        // Block rendering until AuthState finishes loading; the survey route requires an authenticated context.
        await AuthState.InitializeAsync();

        if (!AuthState.IsAuthenticated)
        {
            // Redirect anonymous users to the dedicated 403 page where they can sign in or register.
            ShowForm = false;
            NavigationManager.NavigateTo("/error/403Forbidden", forceLoad: false, replace: true);
            return;
        }

        if (!TryInitializeNavigationContext(out var context))
        {
            ShowForm = false;
            _loadErrorMessage = "We couldn't determine which survey to load. Please return to your profile and try again.";
            _isLoading = false;
            return;
        }

        var surveyTask = ProfileSurveyClient.GetProfileSurveyAsync(context.CategoryId);
        var detailsTask = ProfileSurveyClient.GetProfileSurveyDetailsAsync(context.CategoryId);
        var categoriesTask = SurveyCategoriesClient.GetSurveyCategoriesAsync();

        await Task.WhenAll(surveyTask, detailsTask, categoriesTask);

        _surveyDefinition = await surveyTask;
        var surveyDetails = await detailsTask;
        var categories = await categoriesTask;

        _category = categories.FirstOrDefault(category => category.CategoryId == context.CategoryId);
        _navigationContext = context;
        _isReturningUser = surveyDetails?.Responses is { Count: > 0 };

        if (_surveyDefinition is null || !_surveyDefinition.HasQuestions)
        {
            ShowForm = false;
            _loadErrorMessage = "This survey is currently unavailable. Please try again later.";
            _isLoading = false;
            return;
        }

        ShowForm = true;
        _hasStartedSurvey = false;
        _surveyRendered = false;
        _isLoading = false;
        _loadErrorMessage = null;
        _surveyCompleted = false;
        _formattedResponsesJson = null;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (ShouldShowSurveyContainer && !_surveyRendered)
        {
            await RenderSurvey();
        }
    }

    private async Task RenderSurvey()
    {
        if (_surveyDefinition is null)
        {
            return;
        }

        _dotNetReference ??= DotNetObjectReference.Create(this);

        await JS.InvokeVoidAsync("renderSurvey", _surveyDefinition, "surveyContainer", _dotNetReference);
        _surveyRendered = true;
    }

    private async Task ContinueToSurvey()
    {
        if (!CanContinue)
        {
            return;
        }

        _hasStartedSurvey = true;
        _surveyRendered = false;
        _surveyCompleted = false;
        _formattedResponsesJson = null;
        _rewardResponse = null;

        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task OnSurveyComplete(string responsesJson)
    {
        if (string.IsNullOrWhiteSpace(responsesJson))
        {
            return;
        }

        _surveyCompleted = true;
        _hasStartedSurvey = false;
        _surveyRendered = false;
        _formattedResponsesJson = FormatJson(responsesJson);
        var rewardResponse = await SaveSurveyResultsAsync(responsesJson);
        _rewardResponse = rewardResponse ?? new EngageRewardResponseModel();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _dotNetReference?.Dispose();
    }

    private bool TryInitializeNavigationContext(out SurveyNavigationContext context)
    {
        context = default!;

        var absoluteUri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

        if (string.IsNullOrWhiteSpace(absoluteUri.Query))
        {
            return false;
        }

        var queryParams = QueryHelpers.ParseQuery(absoluteUri.Query);

        if (!queryParams.TryGetValue("c", out var value) || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var encodedContext = value.ToString();

        if (!SurveyNavigationContext.TryDecode(encodedContext, out context) || context is null)
        {
            return false;
        }

        _navigationContext = context;

        return true;

    }

    private async Task<EngageRewardResponseModel?> SaveSurveyResultsAsync(string responsesJson)
    {
        if (_navigationContext is null)
        {
            return null;
        }

        var request = new SaveSurveyResultsRequest
        {
            CategoryId = _navigationContext.CategoryId,
            SurveyJson = responsesJson
        };

        try
        {
            return await SurveyResultsClient.SaveSurveyResultsAsync(request);
        }
        catch
        {
            // Swallow exceptions to avoid breaking the completion view.
            return null;
        }
    }

    private void NavigateToCategoryDetails()
    {
        if (_navigationContext is null)
        {
            return;
        }

        NavigationManager.NavigateTo($"/profile/categories/{_navigationContext.CategoryId}");
    }

    private static string FormatJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return json;
        }
    }

    private static string FormatPoints(int points)
    {
        var normalized = points < 0 ? 0 : points;
        return normalized.ToString("N0", CultureInfo.InvariantCulture);
    }

    private int GetExpectedRewardPoints()
    {
        if (_category is null)
        {
            return 0;
        }

        return _isReturningUser ? _category.RetakePoints : _category.RewardPoints;
    }
}
