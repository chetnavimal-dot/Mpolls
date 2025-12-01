using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MPolls.WebUI.Models.Auth;
using MPolls.WebUI.Services;
using MudBlazor;

namespace MPolls.WebUI.Layout;

public partial class NavMenu : IDisposable
{
    [Inject] private AuthState AuthState { get; set; } = default!;
    [Inject] private AuthClient AuthClient { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private bool IsAuthenticated => AuthState.IsAuthenticated;
    private bool DisableResendButton => _isSendingVerification || !CanResendVerification;
    private string ResendVerificationLabel => _isSendingVerification ? "Sending Email..." : "Resend Email";
    private bool ShowVerificationAlert => IsAuthenticated && CanResendVerification;
    private string SupportEmail => "support@mpolls.com";
    private int CurrentYear => DateTime.UtcNow.Year;

    private bool _isSendingVerification;

    private bool CanResendVerification
    {
        get
        {
            var currentUser = AuthState.CurrentUser;
            return currentUser is not null && !currentUser.vFlag;
        }
    }

    protected override void OnInitialized()
    {
        // Keep the navigation links in sync with whichever user AuthState eventually loads.
        AuthState.AuthenticationStateChanged += HandleAuthenticationStateChanged;
    }

    private void HandleAuthenticationStateChanged()
    {
        // Event callbacks may come from background tasks; marshal back onto the renderer thread.
        InvokeAsync(StateHasChanged);
    }

    private async Task ResendVerificationEmail(MouseEventArgs args)
    {
        if (_isSendingVerification)
        {
            return;
        }

        var currentUser = AuthState.CurrentUser;

        if (currentUser is null)
        {
            // No authenticated session means the resend endpoint would fail—surface a friendly message instead.
            Snackbar.Add("Unable to send a verification email without a valid session.", Severity.Error);
            return;
        }

        _isSendingVerification = true;
        StateHasChanged();

        try
        {
            var response = await AuthClient.ResendEmailVerificationAsync(new EmailVerificationRequest());

            if (response.Payload is not null)
            {
                Snackbar.Add(response.Payload.Message, Severity.Success);
            }
            else if (response.Error is not null)
            {
                var message = string.IsNullOrWhiteSpace(response.Error.Details)
                    ? response.Error.Message
                    : $"{response.Error.Message} ({response.Error.Details})";

                Snackbar.Add(message, Severity.Error);
            }
            else
            {
                Snackbar.Add("No response from the server.", Severity.Error);
            }
        }
        finally
        {
            _isSendingVerification = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        AuthState.AuthenticationStateChanged -= HandleAuthenticationStateChanged;
    }
}
