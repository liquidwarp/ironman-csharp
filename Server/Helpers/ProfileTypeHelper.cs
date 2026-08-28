namespace IronManServer.Helpers;

using System.Collections.Concurrent;
using Models.Enums;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;

[Injectable]
public class ProfileTypeHelper(ProfileHelper profileHelper)
{
    private readonly ConcurrentDictionary<MongoId, ProfileType> _profileTypes = new();

    public bool IsIronmanProfile(MongoId sessionId)
    {
        return IsIronmanProfile(GetProfileType(sessionId));
    }

    public bool IsIronmanProfile(ProfileType profileType)
    {
        return profileType is ProfileType.Standard or ProfileType.Ultimate or ProfileType.Hardcore;
    }

    public bool IsStandardProfile(MongoId sessionId)
    {
        return IsStandardProfile(GetProfileType(sessionId));
    }

    public bool IsStandardProfile(ProfileType profileType)
    {
        return profileType == ProfileType.Standard;
    }

    public bool IsUltimateProfile(MongoId sessionId)
    {
        return IsUltimateProfile(GetProfileType(sessionId));
    }

    public bool IsUltimateProfile(ProfileType profileType)
    {
        return profileType == ProfileType.Ultimate;
    }

    public bool IsHardcoreProfile(MongoId sessionId)
    {
        return IsHardcoreProfile(GetProfileType(sessionId));
    }

    public bool IsHardcoreProfile(ProfileType profileType)
    {
        return profileType == ProfileType.Hardcore;
    }

    public ProfileType GetProfileType(MongoId sessionId)
    {
        return _profileTypes.GetOrAdd(sessionId, id => ParseProfileType(profileHelper.GetFullProfile(id).ProfileInfo?.Edition));
    }

    private void SetProfileType(MongoId sessionId, ProfileType profileType)
    {
        _profileTypes[sessionId] = profileType;

        var realProfile = profileHelper.GetFullProfile(sessionId);
        realProfile.ProfileInfo?.Edition = profileType switch
        {
            ProfileType.Standard => "Standard Ironman",
            ProfileType.Ultimate => "Ultimate Ironman",
            ProfileType.Hardcore => "Hardcore Ironman",
            _ => realProfile.ProfileInfo.Edition
        };
    }
    
    public ProfileType DowngradeProfileType(MongoId sessionId)
    {
        var currentType = GetProfileType(sessionId);

        var downgradedType = currentType switch
        {
            ProfileType.Hardcore => ProfileType.Ultimate,
            ProfileType.Ultimate or ProfileType.Standard => ProfileType.Standard,
            _ => ProfileType.None
        };

        SetProfileType(sessionId, downgradedType);

        return downgradedType;
    }

    private static ProfileType ParseProfileType(string? edition)
    {
        return edition?.ToLowerInvariant() switch
        {
            "standard ironman" => ProfileType.Standard,
            "ultimate ironman" => ProfileType.Ultimate,
            "hardcore ironman" => ProfileType.Hardcore,
            _ => ProfileType.None
        };
    }
}
