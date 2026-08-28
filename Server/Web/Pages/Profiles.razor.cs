namespace IronManServer.Web.Pages;

using IronManServer.Models.Enums;
using Models;
using Microsoft.AspNetCore.Components;
using Services;

public partial class Profiles
{
    [Inject]
    private WebProfileStatusService WebProfileStatusService { get; set; } = default!;

    private ProfileType? _selectedFilter;

    private ProfileStatusInfo? _selectedProfile;

    private IReadOnlyList<ProfileStatusInfo> _profiles = [];

    protected override void OnInitialized()
    {
        LoadProfiles();
    }

    private void LoadProfiles()
    {
        _profiles = WebProfileStatusService.GetAll();
    }

    private IEnumerable<ProfileStatusInfo> FilteredProfiles
    {
        get => _selectedFilter is null ? _profiles : _profiles.Where(profile => profile.ProfileType == _selectedFilter.Value);
    }

    private void SelectFilter(ProfileType? profileType)
    {
        _selectedFilter = profileType;
        _selectedProfile = null;
    }

    private void SelectProfile(ProfileStatusInfo profile)
    {
        _selectedProfile = WebProfileStatusService.Get(profile.ProfileId);
    }

    private void ReturnToProfiles()
    {
        _selectedProfile = null;
    }

    private string GetFilterClass(ProfileType? profileType)
    {
        return _selectedFilter == profileType ? "active" : string.Empty;
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("N0");
    }

    private static string FormatPercentage(double value)
    {
        return $"{value:0.0}%";
    }
    
    private static string FormatAchievementDate(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().ToString("MMM d, yyyy");
    }
}