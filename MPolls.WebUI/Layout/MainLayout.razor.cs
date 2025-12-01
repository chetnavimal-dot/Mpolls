using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MPolls.WebUI.Services;
using MudBlazor;

namespace MPolls.WebUI.Layout;

public partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject] private AuthState AuthState { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private bool _drawerOpen = true;

    private bool IsAuthenticated => AuthState.IsAuthenticated;
    private string UserEmail => AuthState.CurrentUser?.Email ?? string.Empty;

    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;

    protected override void OnInitialized()
    {
        // Subscribe immediately so layout chrome responds when AuthState finishes loading.
        AuthState.AuthenticationStateChanged += HandleAuthenticationStateChanged;
    }

    private void HandleAuthenticationStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task LogoutAsync(MouseEventArgs args)
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true,
            CloseOnEscapeKey = true
        };

        bool? confirm = await DialogService.ShowMessageBox(
            "Confirm logout",
            "Are you sure you want to sign out?",
            yesText: "Logout",
            cancelText: "Cancel",
            options: options);

        if (confirm != true)
        {
            return;
        }

        // Clearing the server session also clears AuthState, so redirect to a public landing page.
        await AuthState.Logout();
        NavigationManager.NavigateTo("/", forceLoad: false, replace: true);
    }

    public void Dispose()
    {
        AuthState.AuthenticationStateChanged -= HandleAuthenticationStateChanged;
    }
}
