using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Services;

namespace MPolls.WebUI;

public partial class App : ComponentBase
{
    [Inject] private AuthState AuthState { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // Load the persisted session so routing decisions reflect the signed-in user immediately.
        await AuthState.InitializeAsync();
    }
}
