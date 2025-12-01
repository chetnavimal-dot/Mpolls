using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Models;
using MudBlazor;

namespace MPolls.WebUI.Components;

public partial class EmployeeDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public UpdateEmployeeCommand Model { get; set; } = new();

    private MudForm? _form;

    private async Task Submit()
    {
        await _form!.Validate();
        if (_form.IsValid)
        {
            MudDialog.Close(DialogResult.Ok(Model));
        }
    }

    private void Cancel() => MudDialog.Cancel();
}

