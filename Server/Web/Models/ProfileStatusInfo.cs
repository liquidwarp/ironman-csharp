namespace IronManServer.Web.Models;

using IronManServer.Models.Enums;
using SPTarkov.Server.Core.Models.Common;

public sealed class ProfileStatusInfo
{
    public required MongoId ProfileId { get; init; }

    public required ProfileType ProfileType { get; init; }

    public required string ProfileName { get; init; }
    public required string ProfileSubtitle { get; init; }
    public required string CssModifier { get; init; }

    public required string Username { get; init; }
    public required string Nickname { get; init; }

    public int Level { get; init; }

    public ExperienceInfo? Experience { get; init; }

    public double Roubles { get; init; }
    public double Dollars { get; init; }
    public double Euros { get; init; }
    public double GpCoins { get; init; }

    public double Health { get; init; }
    public double MaxHealth { get; init; }

    public double Energy { get; init; }
    public double Hydration { get; init; }

    public double Raids { get; init; }
    public double SurvivedRaids { get; init; }
    public double Deaths { get; init; }
    public double Kills { get; init; }

    public IReadOnlyList<AchievementInfo> Achievements { get; init; } = [];

    public double SurvivalRate
    {
        get => Raids <= 0 ? 0 : SurvivedRaids / Raids * 100;
    }

    public record AchievementInfo(AchievementType Type, long DateAchieved, int LevelAchieved);

    public record ExperienceInfo(int Experience, int CurrentLevelExperience, int NextLevelExperience)
    {
        public int ExperienceIntoLevel
        {
            get => Math.Max(0, Experience - CurrentLevelExperience);
        }

        public int ExperienceRequiredForLevel
        {
            get => Math.Max(0, NextLevelExperience - CurrentLevelExperience);
        }

        public double Progress
        {
            get => ExperienceRequiredForLevel <= 0 ? 100 : Math.Clamp(ExperienceIntoLevel / (double)ExperienceRequiredForLevel * 100, 0, 100);
        }
    }
}