namespace IronManServer.Helpers;

using Models.Enums;

public static class WebAchievementHelper
{
    public static string GetTitle(this AchievementType achievement)
    {
        return achievement switch
        {
            AchievementType.BackOnYourFeet => "Back on Your Feet",
            AchievementType.CowardsWayOut => "Coward's Way Out",
            AchievementType.TooHardMan => "Too Hard, Man",
            AchievementType.NothingPersonal => "Nothing Personal",
            AchievementType.StillStanding => "Still Standing",
            AchievementType.NoQuarter => "No Quarter",
            AchievementType.FromTheAshes => "From the Ashes",
            AchievementType.OneLife => "One Life",
            _ => achievement.ToString()
        };
    }

    public static string GetDescription(this AchievementType achievement)
    {
        return achievement switch
        {
            AchievementType.BackOnYourFeet => "Your first iron death. What is dead may never die.",
            AchievementType.CowardsWayOut => "Downgrade from Hardcore to Ultimate.",
            AchievementType.TooHardMan => "Downgrade from Ultimate to Standard.",
            AchievementType.NothingPersonal => "Lose an item worth at least ₽1,000,000 on death.",
            AchievementType.StillStanding => "Reach level 20 after dying at least 10 times.",
            AchievementType.NoQuarter => "Reach level 40 after having had no roubles following a prior death.",
            AchievementType.FromTheAshes => "Lose at least ₽10,000,000 worth of gear and reach level 40.",
            AchievementType.OneLife => "Reach level 60 without dying.",
            _ => string.Empty
        };
    }
}
