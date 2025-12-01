using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MPolls.WebUI.Components.Modals;

public partial class AlertDialog : ComponentBase
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public string ContentText { get; set; }
    [Parameter] public string ButtonText { get; set; } = "OK";

    private void Ok() => MudDialog.Close(DialogResult.Ok(true));
}