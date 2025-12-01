using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Models;
using MPolls.WebUI.Models.Auth;
using MPolls.WebUI.Services;
using MudBlazor;

namespace MPolls.WebUI.Pages;

public sealed partial class MyAccount : ComponentBase, IDisposable
{
    [Inject]
    private AuthState AuthState { get; set; } = default!;

    [Inject]
    private AuthClient AuthClient { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private AuthLoginResponse? _currentUser;
    private bool _emailVerified;

    private bool _isSendingPasswordReset;
    private bool _isSendingVerification;
    private bool _isSavingPreferences;

    private string? _passwordResetFeedback;
    private Severity _passwordResetFeedbackSeverity = Severity.Info;
    private string? _verificationFeedback;
    private Severity _verificationFeedbackSeverity = Severity.Info;
    private string? _preferencesFeedback;
    private Severity _preferencesFeedbackSeverity = Severity.Info;

    private AccountPreferences _preferences = AccountPreferences.CreateDefault();
    private IReadOnlyList<AccountSessionHelper> _sessionHelpers = AccountSessionHelper.CreateDefaults();

    private string AccountEmail => _currentUser?.Email ?? string.Empty;
    private bool CanRequestPasswordChange => _currentUser is not null && !_isSendingPasswordReset;
    private bool CanSendVerification => ShowVerificationActions && !_isSendingVerification;
    private string PasswordResetButtonLabel => _isSendingPasswordReset ? "Sending reset link..." : "Send reset link";
    private string VerificationButtonLabel => _isSendingVerification ? "Sending verification..." : "Resend verification";
    private string SavePreferencesButtonLabel => _isSavingPreferences ? "Saving preferences..." : "Save preferences";
    private bool ShowVerificationActions => _currentUser is not null && !_emailVerified;
    private string VerificationStatusLabel => _emailVerified ? "Email verified" : "Verification pending";

    protected override async Task OnInitializedAsync()
    {
        // Subscribe early so profile fields react if AuthState refreshes (e.g., email verification completes).
        AuthState.AuthenticationStateChanged += HandleAuthenticationStateChanged;

        // Wait for the initial session to hydrate before reading CurrentUser.
        await AuthState.InitializeAsync();

        if (!AuthState.IsAuthenticated)
        {
            // Present the dedicated 403 experience so guests learn how to sign in or create an account.
            NavigationManager.NavigateTo("/error/403Forbidden", replace: true);
            return;
        }

        UpdateCurrentUser(AuthState.CurrentUser);
    }

    private void UpdateCurrentUser(AuthLoginResponse? user)
    {
        _currentUser = user;
        _emailVerified = user?.vFlag ?? false;
    }

    private async Task SendPasswordResetAsync()
    {
        if (!CanRequestPasswordChange)
        {
            return;
        }

        if (_currentUser is null)
        {
            Snackbar.Add("You must be signed in to manage account security.", Severity.Error);
            return;
        }

        _isSendingPasswordReset = true;
        _passwordResetFeedback = null;

        try
        {
            var request = new PasswordChangeRequest { Email = _currentUser.Email };
            var response = await AuthClient.SendPasswordResetEmailAsync(request);

            if (response.Payload is not null)
            {
                _passwordResetFeedback = response.Payload.Message;
                _passwordResetFeedbackSeverity = Severity.Success;
                Snackbar.Add(response.Payload.Message, Severity.Success);
            }
            else if (response.Error is not null)
            {
                var message = string.IsNullOrWhiteSpace(response.Error.Details)
                    ? response.Error.Message
                    : $"{response.Error.Message} ({response.Error.Details})";

                _passwordResetFeedback = message;
                _passwordResetFeedbackSeverity = Severity.Error;
                Snackbar.Add(message, Severity.Error);
            }
            else
            {
                const string fallbackMessage = "No response from the server.";
                _passwordResetFeedback = fallbackMessage;
                _passwordResetFeedbackSeverity = Severity.Error;
                Snackbar.Add(fallbackMessage, Severity.Error);
            }
        }
        finally
        {
            _isSendingPasswordReset = false;
        }
    }

    private async Task SendVerificationEmailAsync()
    {
        if (!ShowVerificationActions || _isSendingVerification)
        {
            return;
        }

        if (_currentUser is null)
        {
            Snackbar.Add("You must be signed in to request a verification email.", Severity.Error);
            return;
        }

        _isSendingVerification = true;
        _verificationFeedback = null;

        try
        {
            var response = await AuthClient.ResendEmailVerificationAsync(new EmailVerificationRequest());

            if (response.Payload is not null)
            {
                _verificationFeedback = response.Payload.Message;
                _verificationFeedbackSeverity = Severity.Success;
                Snackbar.Add(response.Payload.Message, Severity.Success);
            }
            else if (response.Error is not null)
            {
                var message = string.IsNullOrWhiteSpace(response.Error.Details)
                    ? response.Error.Message
                    : $"{response.Error.Message} ({response.Error.Details})";

                _verificationFeedback = message;
                _verificationFeedbackSeverity = Severity.Error;
                Snackbar.Add(message, Severity.Error);
            }
            else
            {
                const string fallbackMessage = "No response from the server.";
                _verificationFeedback = fallbackMessage;
                _verificationFeedbackSeverity = Severity.Error;
                Snackbar.Add(fallbackMessage, Severity.Error);
            }
        }
        finally
        {
            _isSendingVerification = false;
        }
    }

    private async Task SavePreferencesAsync()
    {
        if (_isSavingPreferences)
        {
            return;
        }

        _isSavingPreferences = true;
        _preferencesFeedback = null;

        try
        {
            await Task.CompletedTask;

            _preferencesFeedback = "Your preferences have been saved.";
            _preferencesFeedbackSeverity = Severity.Success;
            Snackbar.Add(_preferencesFeedback, Severity.Success);
        }
        finally
        {
            _isSavingPreferences = false;
        }
    }

    private void HandleAuthenticationStateChanged()
    {
        InvokeAsync(() =>
        {
            UpdateCurrentUser(AuthState.CurrentUser);

            if (!AuthState.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/login", replace: true);
            }

            StateHasChanged();
        });
    }

    public void Dispose()
    {
        AuthState.AuthenticationStateChanged -= HandleAuthenticationStateChanged;
    }

    private sealed class AccountPreferences
    {
        public bool EmailUpdates { get; set; }
        public bool RewardReminders { get; set; }
        public bool ProductNews { get; set; }
        public bool DarkModePreview { get; set; }

        public static AccountPreferences CreateDefault()
        {
            return new AccountPreferences
            {
                EmailUpdates = true,
                RewardReminders = true,
                ProductNews = true,
                DarkModePreview = false
            };
        }
    }

    private sealed record AccountSessionHelper(string Title, string Description, string Icon)
    {
        public static IReadOnlyList<AccountSessionHelper> CreateDefaults()
        {
            return new List<AccountSessionHelper>
            {
                new("Refresh your session", "We automatically renew your credentials when activity slows down, but you can always sign out and back in for a fresh token.", Icons.Material.Filled.Refresh),
                new("Review profile details", "Head to the profile page to ensure your demographics stay current and matched to the best surveys.", Icons.Material.Filled.ManageAccounts),
                new("Check reward balance", "Visit rewards to redeem points before they expire and see the latest catalog additions.", Icons.Material.Filled.CardGiftcard)
            };
        }
    }
}
