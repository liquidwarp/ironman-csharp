namespace IronManServer.Callbacks;

using System.Text.Json;
using System.Text.Json.Serialization;
using Helpers;
using Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;

[Injectable]
public class StaticRouterCallbacks(
    ProfileTypeHelper profileTypeHelper,
    ProfileStatusHelper profileStatusHelper,
    ProfileHelper profileHelper)
{
    public ValueTask<string> GetProfile(string url, EmptyRequestData _, MongoId sessionId)
    {
        var response = new DowngradeResponse
        {
            ProfileType = profileTypeHelper.GetProfileType(sessionId),
            DowngradeLastOfferedAt = profileStatusHelper.GetLastOfferedDowngrade(sessionId)
        };
        
        return new ValueTask<string>(JsonSerializer.Serialize(response, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() }}));
    }
    
    public ValueTask<string> AcceptDowngrade(string url, EmptyRequestData _, MongoId sessionId)
    {
        var previousProfileType = profileTypeHelper.GetProfileType(sessionId);
        var newProfileType = profileTypeHelper.DowngradeProfileType(sessionId);
        
        var profile = profileHelper.GetPmcProfile(sessionId);
        var level = profile?.Info?.Level ?? 0;
        profileStatusHelper.RecordDowngrade(sessionId, previousProfileType, newProfileType, level);
        
        var lastOffered = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        profileStatusHelper.SetLastOfferedDowngrade(sessionId, lastOffered);

        var response = new DowngradeResponse
        {
            ProfileType = newProfileType,
            DowngradeLastOfferedAt = lastOffered
        };
        
        return new ValueTask<string>(JsonSerializer.Serialize(response, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() }}));
    }
    
    public ValueTask<string> DeclineDowngrade(string url, EmptyRequestData _, MongoId sessionId)
    {
        var lastOffered = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        profileStatusHelper.SetLastOfferedDowngrade(sessionId, lastOffered);

        var response = new DowngradeResponse
        {
            ProfileType = profileTypeHelper.GetProfileType(sessionId),
            DowngradeLastOfferedAt = lastOffered
        };
        
        return new ValueTask<string>(JsonSerializer.Serialize(response, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() }}));
    }
}
