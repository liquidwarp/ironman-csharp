namespace IronManServer.Helpers;

using Models;
using Models.Enums;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.Preload)]
public class ProfileStatusHelper(
    ModHelper modHelper,
    JsonUtil jsonUtil,
    FileUtil fileUtil) : IOnLoad
{
    private string _filePath = null!;
    private ProfileStatus _state = null!;

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        _filePath = Path.Combine(
            modHelper.GetAbsolutePathToModFolder(),
            "Data",
            "profiles-status.json");

        _state = Load();

        return Task.CompletedTask;
    }

    public long GetLastOfferedDowngrade(MongoId sessionId)
    {
        return _state.DowngradeLastOfferedAt.GetValueOrDefault(sessionId, 0);
    }

    public void SetLastOfferedDowngrade(MongoId sessionId, long timestamp)
    {
        _state.DowngradeLastOfferedAt[sessionId] = timestamp;
        Save();
    }

    public IReadOnlyDictionary<AchievementType, AchievementInfo> GetAchievements(MongoId sessionId)
    {
        return _state.Achievements.GetValueOrDefault(sessionId) ?? new Dictionary<AchievementType, AchievementInfo>();
    }

    private void SetAchievement(MongoId sessionId, AchievementType achievement, int level)
    {
        if (!_state.Achievements.TryGetValue(sessionId, out var achievements))
        {
            achievements = [];
            _state.Achievements[sessionId] = achievements;
        }

        if (achievements.ContainsKey(achievement))
            return;

        achievements[achievement] = new AchievementInfo
        {
            Type = achievement,
            DateAchieved = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            LevelAchieved = level
        };
    }

    public void RecordDeath(MongoId sessionId, int level)
    {
        var deaths = _state.Deaths.GetValueOrDefault(sessionId) + 1;
        _state.Deaths[sessionId] = deaths;

        if (deaths == 1)
            SetAchievement(sessionId, AchievementType.BackOnYourFeet, level);

        CheckStillStanding(sessionId, level);
        CheckNoQuarter(sessionId, level);
        CheckFromTheAshes(sessionId, level);

        Save();
    }

    public void RecordRoubleBalance(MongoId sessionId, double roubles, int level)
    {
        if (roubles <= 0)
            _state.HasReachedZeroRoubles[sessionId] = true;

        CheckNoQuarter(sessionId, level);

        Save();
    }

    public void RecordLostGear(MongoId sessionId, double value, bool lostMillionPlusItem, int level)
    {
        var total = _state.LostGearValue.GetValueOrDefault(sessionId) + value;
        _state.LostGearValue[sessionId] = total;

        if (lostMillionPlusItem)
            SetAchievement(sessionId, AchievementType.NothingPersonal, level);

        CheckFromTheAshes(sessionId, level);

        Save();
    }

    public void RecordLevel(MongoId sessionId, int level)
    {
        if (level >= 60 && _state.Deaths.GetValueOrDefault(sessionId) == 0)
            SetAchievement(sessionId, AchievementType.OneLife, level);

        CheckStillStanding(sessionId, level);
        CheckNoQuarter(sessionId, level);
        CheckFromTheAshes(sessionId, level);

        Save();
    }

    public void RecordDowngrade(MongoId sessionId, ProfileType previousProfileType, ProfileType newProfileType, int level)
    {
        switch (previousProfileType, newProfileType)
        {
            case (ProfileType.Hardcore, ProfileType.Ultimate):
                SetAchievement(sessionId, AchievementType.CowardsWayOut, level);
                break;

            case (ProfileType.Ultimate, ProfileType.Standard):
                SetAchievement(sessionId, AchievementType.TooHardMan, level);
                break;
        }

        Save();
    }

    private void CheckStillStanding(MongoId sessionId, int level)
    {
        if (level < 20)
            return;

        if (_state.Deaths.GetValueOrDefault(sessionId) < 10)
            return;

        SetAchievement(sessionId, AchievementType.StillStanding, level);
    }

    private void CheckNoQuarter(MongoId sessionId, int level)
    {
        if (level < 40)
            return;

        if (_state.HasReachedZeroRoubles.GetValueOrDefault(sessionId))
            return;

        SetAchievement(sessionId, AchievementType.NoQuarter, level);
    }

    private void CheckFromTheAshes(MongoId sessionId, int level)
    {
        if (level < 40)
            return;

        if (_state.LostGearValue.GetValueOrDefault(sessionId) < 10000000)
            return;

        SetAchievement(sessionId, AchievementType.FromTheAshes, level);
    }

    private ProfileStatus Load()
    {
        if (!fileUtil.FileExists(_filePath))
            return new ProfileStatus();

        return jsonUtil.DeserializeFromFile<ProfileStatus>(_filePath) ?? new ProfileStatus();
    }

    private void Save(CancellationToken cancellationToken = default)
    {
        var stateData = jsonUtil.Serialize(_state, true);
        fileUtil.WriteFileAsync(_filePath, stateData ?? throw new InvalidOperationException(), cancellationToken);
    }
}