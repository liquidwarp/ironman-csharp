namespace IronManServer.Models;

using Enums;

public class ProfileStatus
{
    public Dictionary<string, long> DowngradeLastOfferedAt { get; set; } = [];

    public Dictionary<string, Dictionary<AchievementType, AchievementInfo>> Achievements { get; set; } = [];

    public Dictionary<string, int> Deaths { get; set; } = [];

    public Dictionary<string, double> LostGearValue { get; set; } = [];

    public Dictionary<string, bool> HasReachedZeroRoubles { get; set; } = [];
}