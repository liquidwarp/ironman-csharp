namespace IronManServer.Web.Pages;

using System.Net.Http.Json;
using System.Reflection;
using Helpers;
using Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Utils;
using IronManServer.Config;
using SPTarkov.Common.Models.Logging;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

public partial class Home
{
    private const int ReleasesPageSize = 5;
    
    [Inject] private ISptLogger<Home> Logger { get; set; } = null!;
    [Inject] private FileUtil FileUtil { get; set; } = null!;
    [Inject] private JsonUtil JsonUtil { get; set; } = null!;
    [Inject] private ModHelper ModHelper { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private ConfigManager ConfigManager { get; set; } = null!;

    private IReadOnlyList<ProfileInfo> Profiles
    {
        get => WebProfileHelper.Profiles;
    }

    private List<ReleaseNote>? _releaseNotes;

    private int _releasesPage = 1;

    private List<ReleaseNote> FilteredReleases
    {
        get => GetFilteredReleases();
    }

    private int TotalReleasePages
    {
        get => (int)Math.Ceiling((_releaseNotes?.Count ?? 0) / (double)ReleasesPageSize);
    }

    private bool CanGoToPreviousReleasesPage
    {
        get => _releasesPage > 1;
    }

    private bool CanGoToNextReleasesPage
    {
        get => _releasesPage < TotalReleasePages;
    }

    private bool _updateCheckInProgress;
    private bool _updateAvailable;
    private string? _updateUrl;
    private string? _updateReleaseNotesText;


    protected override async Task OnInitializedAsync()
    {
        ConfigManager.OnChange += HandleConfigChanged;

        if (ConfigManager.RuntimeConfig.ConfigAppSettings.AllowUpdateChecks)
        {
            _updateCheckInProgress = true;

            await CheckForUpdate();

            _updateCheckInProgress = false;
        }

        await LoadReleaseNotes();
        await base.OnInitializedAsync();
    }
    
    private void HandleConfigChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ConfigManager.OnChange -= HandleConfigChanged;
    }

    private async Task LoadReleaseNotes()
    {
        var modPath = ModHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var path = Path.Combine(modPath, "wwwroot", "files", "ReleaseNotes.json");

        var text = await FileUtil.ReadFileAsync(path);

        _releaseNotes = JsonUtil.Deserialize<List<ReleaseNote>>(text)!;
    }


    private List<ReleaseNote> GetFilteredReleases()
    {
        return (_releaseNotes ?? []).Skip((_releasesPage - 1) * ReleasesPageSize).Take(ReleasesPageSize).ToList();
    }


    private async Task GoToPreviousReleasesPage()
    {
        if (!CanGoToPreviousReleasesPage)
            return;

        _releasesPage--;

        await ScrollReleasesToTop();
    }


    private async Task GoToNextReleasesPage()
    {
        if (!CanGoToNextReleasesPage)
            return;

        _releasesPage++;

        await ScrollReleasesToTop();
    }


    private async Task ScrollReleasesToTop()
    {
        await JsRuntime.InvokeVoidAsync("eval", "document.querySelector('.ironman-releases-scroll')?.scrollTo({ top: 0, behavior: 'instant' });");
    }


    private async Task CheckForUpdate()
    {
        try
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("ironman-server");
            httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

            var release = await httpClient.GetFromJsonAsync<ReleaseInformation>("https://api.github.com/repos/acidphantasm/ironman-csharp/releases/latest");

            if (release is null)
                return;

            Version latestVersion = new(release.Version);
            Version currentVersion = new ModMetadata().Version;
            Range currentVersionRange = new($"^{currentVersion.Major}.0.0");
            
            if (!currentVersionRange.IsSatisfied(latestVersion))
                return;

            if (latestVersion > currentVersion)
            {
                _updateAvailable = true;
                _updateUrl = release.DownloadUrl;

                if (!string.IsNullOrWhiteSpace(release.Body))
                {
                    _updateReleaseNotesText = release.Body;
                }
            }
        }
        catch
        {
            // Update checks are not critical.
            // Ignore failures so the homepage still loads normally.
        }
    }
}