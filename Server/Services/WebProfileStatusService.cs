namespace IronManServer.Services;

using Helpers;
using IronManServer.Web.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;

[Injectable]
public sealed class WebProfileStatusService(
    ProfileTypeHelper profileTypeHelper,
    ProfileStatusHelper profileStatusHelper,
    ProfileHelper profileHelper)
{
    public IReadOnlyList<ProfileStatusInfo> GetAll()
    {
        return profileHelper
            .GetProfiles()
            .Values
            .Select(Create)
            .Where(profile => profile is not null)
            .Cast<ProfileStatusInfo>()
            .OrderBy(profile => profile.ProfileType)
            .ThenBy(profile => profile.Nickname)
            .ToList();
    }

    public ProfileStatusInfo? Get(MongoId profileId)
    {
        if (profileHelper.GetProfileByPmcId(profileId) is null)
            return null;

        var profile = profileHelper.GetFullProfile(profileId);

        return Create(profile);
    }

    private ProfileStatusInfo? Create(SptProfile profile)
    {
        var profileId = profile.ProfileInfo?.ProfileId;

        if (profileId is null)
            return null;

        if (!profileTypeHelper.IsIronmanProfile(profileId.Value))
            return null;

        var profileType = profileTypeHelper.GetProfileType(profileId.Value);
        var profileInfo = WebProfileHelper.GetInfo(profileType);

        var pmc = profile.CharacterData?.PmcData;

        if (pmc is null)
            return null;

        var info = pmc.Info;

        if (info is null)
            return null;
        
        var experienceInfo = GetExperienceInfo(pmc);
        var raids = GetCounter(pmc, "Sessions", "Pmc");
        var deaths = GetCounter(pmc, "Deaths");
        var kills = GetCounter(pmc, "Kills");
        var survivedRaids = Math.Max(0, raids - deaths);

        var achievements = profileStatusHelper
            .GetAchievements(profileId.Value)
            .Values
            .Select(achievement => new ProfileStatusInfo.AchievementInfo(
            achievement.Type,
            achievement.DateAchieved,
            achievement.LevelAchieved))
            .OrderBy(achievement => achievement.DateAchieved)
            .ToList();

        return new ProfileStatusInfo
        {
            ProfileId = profileId.Value,
            ProfileType = profileType,
            ProfileName = profileInfo.Name,
            ProfileSubtitle = profileInfo.Subtitle,
            CssModifier = profileInfo.CssModifier,
            Username = info.Nickname ?? "unknown",
            Nickname = info.Nickname ?? "unknown",
            Level = info.Level ?? 0,
            Experience = experienceInfo,
            
            Roubles = profile.CharacterData?.PmcData?.Inventory?.Items?
                .Where(item => item.Template == "5449016a4bdc2d6f028b456f")
                .Sum(item => item.Upd?.StackObjectsCount ?? 0) ?? 0,

            Dollars = profile.CharacterData?.PmcData?.Inventory?.Items?
                .Where(item => item.Template == "569668774bdc2da2298b4568")
                .Sum(item => item.Upd?.StackObjectsCount ?? 0) ?? 0,

            Euros = profile.CharacterData?.PmcData?.Inventory?.Items?
                .Where(item => item.Template == "5696686a4bdc2da3298b456a")
                .Sum(item => item.Upd?.StackObjectsCount ?? 0) ?? 0,

            GpCoins = profile.CharacterData?.PmcData?.Inventory?.Items?
                .Where(item => item.Template == "5d235b4d86f7742eac2b2b9a")
                .Sum(item => item.Upd?.StackObjectsCount ?? 0) ?? 0,
            
            Health = profile.CharacterData?.PmcData?.Health?.BodyParts?
                .Sum(x => x.Value.Health?.Current ?? 0) ?? 0,

            MaxHealth = profile.CharacterData?.PmcData?.Health?.BodyParts?
                .Sum(x => x.Value.Health?.Maximum ?? 0) ?? 0,

            Energy = profile.CharacterData?.PmcData?.Health?.Energy?.Current ?? 0,
            Hydration = profile.CharacterData?.PmcData?.Health?.Hydration?.Current ?? 0,
            Raids = raids,
            SurvivedRaids = survivedRaids,
            Deaths = deaths,
            Kills = kills,
            Achievements = achievements
        };
    }
    
    private ProfileStatusInfo.ExperienceInfo GetExperienceInfo(PmcData pmc)
    {
        var level = pmc.Info?.Level ?? 1;
        var experience = pmc.Info?.Experience ?? 0;

        var currentLevelExperience = profileHelper.GetExperience(level) ?? 0;

        var nextLevelExperience = profileHelper.GetExperience(level + 1) ?? currentLevelExperience;

        return new ProfileStatusInfo.ExperienceInfo(experience, currentLevelExperience, nextLevelExperience);
    }
    
    private static double GetCounter(PmcData pmc, params string[] keys)
    {
        return pmc.Stats?.Eft?.OverallCounters?.Items?.FirstOrDefault(counter => counter.Key is not null && counter.Key.SequenceEqual(keys))?.Value ?? 0;
    }
}
