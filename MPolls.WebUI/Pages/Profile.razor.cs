using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Models;
using MPolls.WebUI.Services;
using MudBlazor;

namespace MPolls.WebUI.Pages;

public partial class Profile : ComponentBase
{
    [Inject]
    private SurveyCategoriesClient SurveyCategoriesClient { get; set; } = default!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AuthState AuthState { get; set; } = default!;

    [Inject]
    private PanelistClient PanelistClient { get; set; } = default!;

    private readonly List<SurveyCategoryModel> _categories = new();
    private bool _isLoading;
    private string? _errorMessage;
    private string? _panelistUlid;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        // Wait until AuthState knows whether a profile exists before attempting to load panelist data.
        await AuthState.InitializeAsync();

        if (!AuthState.IsAuthenticated)
        {
            // Direct anonymous users to the dedicated 403 surface for consistent recovery guidance.
            _isLoading = false;
            NavigationManager.NavigateTo("/error/403Forbidden", forceLoad: false, replace: true);
            return;
        }
        
        if (!AuthState.CurrentUser.IsOnboarded)
        {
            NavigationManager.NavigateTo("/onboarding");
        }

        await EnsurePanelistUlidAsync();

        try
        {
            var categories = await SurveyCategoriesClient.GetSurveyCategoriesAsync();

            _categories.Clear();
            _categories.AddRange(categories);
        }
        catch
        {
            _errorMessage = "We couldn't load your survey categories right now. Please try again later.";
        }

        _isLoading = false;
    }

    private static string GetIcon(SurveyCategoryModel category)
    {
        if (string.IsNullOrWhiteSpace(category.Icon))
            return Icons.Material.Filled.Category;

        var parts = category.Icon.Split('.');

        if (parts.Length != 4)
            return Icons.Material.Filled.Help;

        try
        {
            var parentType = typeof(Icons);

            var materialType = parentType.GetNestedType(parts[1], BindingFlags.Public | BindingFlags.Static);
            if (materialType == null)
                return Icons.Material.Filled.Help;

            var filledType = materialType.GetNestedType(parts[2], BindingFlags.Public | BindingFlags.Static);
            if (filledType == null)
                return Icons.Material.Filled.Help;

            var field = filledType.GetField(parts[3], BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                return Icons.Material.Filled.Help;

            return field.GetValue(null)?.ToString() ?? Icons.Material.Filled.Help;
        }
        catch
        {
            return Icons.Material.Filled.Help;
        }
    }
    
    private void NavigateToCategory(int categoryId)
    {
        NavigationManager.NavigateTo($"/profile/categories/{categoryId}");
    }

    private static string GetRewardDisplay(SurveyCategoryModel category)
    {
        if (category is null)
        {
            return "Not available";
        }

        if (category.RewardPoints <= 0)
        {
            return "Not available";
        }

        return category.RewardPoints.ToString("0 pts", CultureInfo.CurrentCulture);
    }
    
    private static string? GetRetakeDisplay(SurveyCategoryModel category)
    {
        if (category is null)
        {
            return null;
        }

        if (category.RewardPoints <= 0)
        {
            return null;
        }

        return category.RetakePoints.ToString("0 pts", CultureInfo.CurrentCulture);
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

}
