using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MPolls.WebUI.Models;
using MPolls.WebUI.Models.Auth;
using MPolls.WebUI.Services;
using MudBlazor;

namespace MPolls.WebUI.Pages;

public partial class ResetPassword : ComponentBase
{
    [Inject] private AuthClient AuthClient { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthState AuthState { get; set; } = default!;

    private readonly PasswordChangeRequest _model = new();
    private ApiResponse<PasswordChangeResponse> Response { get; set; }
    private readonly EditContext _editContext;
    private bool _isSubmitting;
    private bool ShowSuccessMessage = false;
    private string? _errorMessage;

    public ResetPassword()
    {
        _editContext = new EditContext(_model);
    }

    protected override async Task OnInitializedAsync()
    {
        // Reset flows should behave differently for signed-in users, so wait for AuthState to settle first.
        await AuthState.InitializeAsync();

        if (AuthState.IsAuthenticated)
        {
            // If a session already exists, there's no need for password reset—send them to their dashboard.
            NavigationManager.NavigateTo("/dashboard", forceLoad: false, replace: true);
        }
    }

    private async Task HandleValidSubmit()
    {
        _isSubmitting = true;
        _errorMessage = null;

        try
        {
            var request = new PasswordChangeRequest
            {
                Email = _model.Email.Trim()
            };

            Response = await AuthClient.SendPasswordResetEmailAsync(request);

            string message;
            bool isError;

            if (Response.Error is not null)
            {
                message = string.IsNullOrWhiteSpace(Response.Error.Details)
                    ? Response.Error.Message
                    : $"{Response.Error.Message} ({Response.Error.Details})";
                isError = true;
            }
            else if (Response.Payload is not null)
            {
                message = Response.Payload.Message;
                isError = false;
                ShowSuccessMessage = true;
            }
            else
            {
                message = "No response from the server.";
                isError = true;
            }

            // var options = new DialogOptions
            // {
            //     CloseButton = true,
            //     FullWidth = true,
            //     MaxWidth = MaxWidth.ExtraSmall
            // };
            //
            // await DialogService.ShowMessageBox("Password reset", message, yesText: "OK", options: options);

            _errorMessage = isError ? message : null;
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
