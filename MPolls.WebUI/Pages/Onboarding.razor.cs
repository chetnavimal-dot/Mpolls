using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Models;
using MPolls.WebUI.Models.Panelists;
using MPolls.WebUI.Services;

namespace MPolls.WebUI.Pages;

public partial class Onboarding : ComponentBase
{
    [Inject] private AuthState AuthState { get; set; } = default!;

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Inject] private CountriesClient CountriesClient { get; set; } = default!;

    [Inject] private PanelistClient PanelistClient { get; set; } = default!;

    private bool _isLoading;
    private int CurrentStep = 1;
    private bool IsOnboardingComplete = false;
    private int? Age;
    private int? CountryCode;
    private int? Gender;
    private List<int> Ages = Enumerable.Range(18, 99).ToList();
    private List<CountryModel> Countries = new();
    private CountryModel? SelectedCountry;
    private static readonly IReadOnlyList<KeyValuePair<int, string>> GenderOptions = new List<KeyValuePair<int, string>>
    {
        new(1, "Male"),
        new(2, "Female"),
        new(3, "Other"),
        new(4, "Prefer not to say"),
    };

    private bool CanAdvanceToNextStep => CurrentStep switch
    {
        1 => Age.HasValue,
        2 => CountryCode.HasValue,
        _ => false,
    };

    private bool CanCompleteOnboarding => Age.HasValue && CountryCode.HasValue && Gender.HasValue;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        await AuthState.InitializeAsync();

        if (!AuthState.IsAuthenticated)
        {
            _isLoading = false;
            NavigationManager.NavigateTo("/error/403Forbidden", forceLoad: false, replace: true);
            return;
        }

        Countries = await CountriesClient.GetCountriesAsync();
        IsOnboardingComplete = AuthState.CurrentUser.IsOnboarded;

        _isLoading = false;
    }

    private void GoNext()
    {
        if (!CanAdvanceToNextStep)
        {
            return;
        }

        if (CurrentStep < 3)
        {
            CurrentStep++;
        }
    }

    private void GoPrevious()
    {
        if (CurrentStep > 1)
            CurrentStep--;
    }

    private async Task CompleteOnboarding()
    {
        if (!CanCompleteOnboarding)
        {
            return;
        }

        var request = new CompleteOnboardingRequest
        {
            Age = Age.Value,
            CountryCode = CountryCode.Value,
            Gender = Gender.Value,
        };

        var completed = await PanelistClient.CompleteOnboardingAsync(request);

        if (completed)
        {
            IsOnboardingComplete = true;
            AuthState.RefreshSessionAsync();
        }
    }

    private async Task OnCountryChanged(int? countryCode)
    {
        CountryCode = countryCode;

        SelectedCountry = null;

        if (countryCode.HasValue)
        {
            SelectedCountry = await CountriesClient.GetCountryAsync(countryCode.Value);
        }
    }
    
    private Task<IEnumerable<int?>> SearchCountries(string country, CancellationToken cancellationToken)
    {
        IEnumerable<int?> result;

        if (string.IsNullOrWhiteSpace(country))
        {
            result = Countries.Select(c => (int?)c.CountryCode);
        }
        else
        {
            result = Countries
                .Where(c => c.CountryName.Contains(country, StringComparison.OrdinalIgnoreCase)
                            || c.CountryShortCode.Contains(country, StringComparison.OrdinalIgnoreCase))
                .Select(c => (int?)c.CountryCode);
        }

        return Task.FromResult(result);
    }

    private string GetCountryDisplay(int? code)
    {
        if (code == null)
            return string.Empty;

        var country = Countries.FirstOrDefault(c => c.CountryCode == code);
        return country != null ? $"{country.CountryName} ({country.CountryShortCode})" : string.Empty;
    }
    
    private Task<IEnumerable<int?>> SearchAges(string age, CancellationToken cancellationToken)
    {
        IEnumerable<int?> result;

        if (string.IsNullOrWhiteSpace(age))
        {
            result = Ages.Select(a => (int?)a);
        }
        else
        {
            result = Ages
                .Where(a => a.ToString().Contains(age, StringComparison.OrdinalIgnoreCase))
                .Select(a => (int?)a);
        }

        return Task.FromResult(result);
    }
}
