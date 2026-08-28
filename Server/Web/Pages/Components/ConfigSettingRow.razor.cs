using Microsoft.AspNetCore.Components;

namespace IronManServer.Web.Pages.Components;

public partial class ConfigSettingRow
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Description { get; set; } = string.Empty;

    [Parameter]
    public bool HasChanged { get; set; }

    [Parameter]
    public bool ShowDefaultButton { get; set; }
    
    [Parameter]
    public bool CanRestoreDefault { get; set; }

    [Parameter]
    public RenderFragment? Control { get; set; }
    
    [Parameter]
    public RenderFragment? NumberInput { get; set; }

    [Parameter]
    public RenderFragment? Slider { get; set; }

    [Parameter]
    public EventCallback OnUndo { get; set; }

    [Parameter]
    public EventCallback OnDefault { get; set; }
}
