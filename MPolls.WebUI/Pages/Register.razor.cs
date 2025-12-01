using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MPolls.WebUI.Models.Auth;
using MPolls.WebUI.Services;

namespace MPolls.WebUI.Pages;

public partial class Register : ComponentBase
{
    [Inject] private AuthClient AuthClient { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthState AuthState { get; set; } = default!;

    private readonly RegisterRequest _model = new();
    private readonly EditContext _editContext;
    private bool _isSubmitting;
    private string? _errorMessage;
    private AuthRegistrationResponse? _registrationResponse;
    private bool HasUserConsented = false;

    public Register()
    {
        _editContext = new EditContext(_model);
    }

    protected override async Task OnInitializedAsync()
    {
        // Prevent already authenticated users from registering a duplicate account.
        await AuthState.InitializeAsync();

        if (AuthState.IsAuthenticated)
        {
            // Signed-in visitors are redirected to their dashboard instead of seeing the registration form.
            NavigationManager.NavigateTo("/dashboard", forceLoad: false, replace: true);
        }
    }

    private async Task HandleValidSubmit()
    {
        _isSubmitting = true;
        _errorMessage = null;
        _registrationResponse = null;

        try
        {
            var request = new AuthRequest
            {
                Email = _model.Email,
                Password = _model.Password
            };

            var response = await AuthClient.RegisterAsync(request);

            if (response.Error is not null)
            {
                _errorMessage = string.IsNullOrWhiteSpace(response.Error.Details)
                    ? response.Error.Message
                    : $"{response.Error.Message} ({response.Error.Details})";
            }
            else if (response.Payload is not null)
            {
                _registrationResponse = response.Payload;
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
