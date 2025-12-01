using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Services;

namespace MPolls.WebUI.Pages;

public partial class Landing : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthState AuthState { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // Determine whether to show marketing content or route the user directly into the app.
        await AuthState.InitializeAsync();

        if (AuthState.IsAuthenticated)
        {
            // Authenticated visitors skip the marketing page and land on their dashboard.
            NavigationManager.NavigateTo("/dashboard", forceLoad: false, replace: true);
        }
    }

    private void NavigateToLogin()
    {
        NavigationManager.NavigateTo("/login");
    }

    private void NavigateToRegister()
    {
        NavigationManager.NavigateTo("/register");
    }
}
