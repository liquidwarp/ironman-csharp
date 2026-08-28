namespace IronManServer.Models;

using Enums;

public class AchievementInfo
{
    public required AchievementType Type { get; init; }
    public required long DateAchieved { get; init; }
    public required int LevelAchieved { get; init; }
}
