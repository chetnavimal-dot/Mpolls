using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MPolls.WebUI.Models.Auth;
using MPolls.WebUI.Services;

namespace MPolls.WebUI.Pages;

public partial class Login : ComponentBase
{
    [Inject] private AuthClient AuthClient { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthState AuthState { get; set; } = default!;

    private readonly AuthRequest _model = new();
    private readonly EditContext _editContext;
    private bool _isSubmitting;
    private string? _errorMessage;
    public Login()
    {
        _editContext = new EditContext(_model);
    }

    protected override async Task OnInitializedAsync()
    {
        // Allow the login page to detect existing sessions and redirect without showing the form.
        await AuthState.InitializeAsync();

        if (AuthState.IsAuthenticated)
        {
            // If a session is already active, head straight to the dashboard instead of forcing another login.
            NavigationManager.NavigateTo("/dashboard", forceLoad: false, replace: true);
        }
    }

    private async Task HandleValidSubmit()
    {
        _isSubmitting = true;
        _errorMessage = null;

        try
        {
            var request = new AuthRequest
            {
                Email = _model.Email.Trim(),
                Password = _model.Password
            };

            var response = await AuthClient.LoginAsync(request);

            if (response.Error is not null)
            {
                _errorMessage = string.IsNullOrWhiteSpace(response.Error.Details)
                    ? response.Error.Message
                    : $"{response.Error.Message} ({response.Error.Details})";
            }
            else if (response.Payload is not null)
            {
                // Persist the authenticated user so the rest of the app observes the new session.
                await AuthState.SetAuthenticatedUser(response.Payload);
                
                NavigationManager.NavigateTo(response.Payload.IsOnboarded ? "/dashboard" : "/onboarding", forceLoad: false, replace: true);
            }
            else
            {
                _errorMessage = "No response from the server.";
            }
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
