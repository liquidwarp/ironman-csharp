namespace IronManClient;

using System;
using BepInEx;
using BepInEx.Logging;
using Patches.Experience;
using Patches.Health;
using Patches.Menu;
using Patches.Player;
using Patches.Scavs;
using Patches.Skills;
using Patches.Trader;
using VersionCheck;

[BepInPlugin("com.acidphantasm.ironman", "acidphantasm-ironman", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private static Plugin _instance;
    public static ManualLogSource Log
    {
        get => _instance.Logger;
    }

    private void Awake()
    {
        if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
        {
            throw new Exception($"Invalid EFT Version");
        }
        
        _instance = this;
        
        new CreatePatch().Enable();
        new IsAvailablePatch().Enable();
        new OnHealthApplyDamagePatch().Enable();
        new OnDeadPatch().Enable();
        new ShowPatch().Enable();
        new OnGameSessionEndPatch().Enable();
        new HealPricePatch().Enable();
        new OnTriggerPatch().Enable();
        new EndStatisticsSessionPatch().Enable();
        new GlobalConfigurationCreatePatch().Enable();
        new ProceedFromKillListPatch().Enable();
        new ItemRepairCostPatch().Enable();
    }
}