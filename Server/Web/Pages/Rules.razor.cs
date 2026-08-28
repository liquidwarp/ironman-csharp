namespace IronManServer.Web.Pages;

using Helpers;
using IronManServer.Models.Enums;
using IronManServer.Web.Models;
using IronManServer.Web.Shared;

public partial class Rules
{
    private ProfileType _selectedProfile = ProfileType.Standard;

    private ProfileInfo SelectedProfileInfo
    {
        get => WebProfileHelper.GetInfo(_selectedProfile);
    }

    private IReadOnlyList<RuleGroup> SelectedRuleGroups
    {
        get => WebProfileHelper.GetRules(_selectedProfile);
    }

    private void SelectProfile(ProfileType profile)
    {
        _selectedProfile = profile;
    }

    private static string GetValueClass(Rule rule)
    {
        return rule.ChangeType switch
        {
            RuleChangeType.Standard => "standard",
            RuleChangeType.Changed => "changed",
            RuleChangeType.Added => "added",
            _ => "standard"
        };
    }
}
