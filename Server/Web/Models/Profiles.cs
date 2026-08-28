namespace IronManServer.Web.Models;

using IronManServer.Models.Enums;

public record ProfileInfo(
    ProfileType Type, 
    string Name, 
    string Subtitle, 
    string Badge, 
    string CssModifier, 
    string Description, 
    int RiskLevel, 
    string RiskName, 
    IReadOnlyList<RuleGroup> RuleGroups);

public record RuleGroup(
    string Name, 
    IReadOnlyList<Rule> Rules);

public record Rule(
    string Name,
    string Value,
    RuleChangeType ChangeType = RuleChangeType.Standard
);

public enum RuleChangeType
{
    Standard,
    Changed,
    Added
}