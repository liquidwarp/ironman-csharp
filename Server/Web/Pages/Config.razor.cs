namespace IronManServer.Web.Pages;

using IronManServer.Config;
using Helpers;
using IronManServer.Models;
using Microsoft.AspNetCore.Components;

public partial class Config
{
    [Inject]
    private ConfigManager ConfigManager { get; set; } = null!;

    [Inject]
    private Utils ImUtils { get; set; } = null!;


    private readonly IronManConfig _defaultConfig = new();
    
    protected override void OnInitialized()
    {
        ConfigManager.OnChange += HandleConfigChanged;
    }

    private void HandleConfigChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ConfigManager.OnChange -= HandleConfigChanged;
    }
    
    private bool ShowDefaultButton
    {
        get => ConfigManager.RuntimeConfig.ConfigAppSettings.ShowDefaultButton;
    }

    private static bool IsChanged<T>(T current, T original)
    {
        return !EqualityComparer<T>.Default.Equals(current, original);
    }


    private void UpdateInt(int value, int originalValue, string changeKey)
    {
        ImUtils.UpdateView(value, originalValue, changeKey);
    }


    private void UpdateBool(bool value, bool originalValue, string changeKey)
    {
        ImUtils.UpdateView(value, originalValue, changeKey);
    }

    private void SetStandardSkillLevelLoss(int value)
    {
        value = Math.Clamp(value, 0, 100);
        ConfigManager.RuntimeConfig.Standard.ChanceOfSkillLevelLoss = value;
        UpdateInt(value, ConfigManager.OriginalConfig.Standard.ChanceOfSkillLevelLoss, nameof(SetStandardSkillLevelLoss));
    }


    private void UndoStandardSkillLevelLoss()
    {
        SetStandardSkillLevelLoss(ConfigManager.OriginalConfig.Standard.ChanceOfSkillLevelLoss);
    }


    private void DefaultStandardSkillLevelLoss()
    {
        SetStandardSkillLevelLoss(_defaultConfig.Standard.ChanceOfSkillLevelLoss);
    }


    private void SetStandardPlayerLevelLoss(int value)
    {
        value = Math.Clamp(value, 0, 100);

        ConfigManager.RuntimeConfig.Standard.ChanceOfPlayerLevelLoss = value;

        UpdateInt(value, ConfigManager.OriginalConfig.Standard.ChanceOfPlayerLevelLoss, nameof(SetStandardPlayerLevelLoss));
    }


    private void UndoStandardPlayerLevelLoss()
    {
        SetStandardPlayerLevelLoss(ConfigManager.OriginalConfig.Standard.ChanceOfPlayerLevelLoss);
    }


    private void DefaultStandardPlayerLevelLoss()
    {
        SetStandardPlayerLevelLoss(_defaultConfig.Standard.ChanceOfPlayerLevelLoss);
    }
    
    private void SetUltimateSkillLevelLoss(int value)
    {
        value = Math.Clamp(value, 0, 100);

        ConfigManager.RuntimeConfig.Ultimate.ChanceOfSkillLevelLoss = value;

        UpdateInt(value, ConfigManager.OriginalConfig.Ultimate.ChanceOfSkillLevelLoss, nameof(SetUltimateSkillLevelLoss));
    }


    private void UndoUltimateSkillLevelLoss()
    {
        SetUltimateSkillLevelLoss(ConfigManager.OriginalConfig.Ultimate.ChanceOfSkillLevelLoss);
    }


    private void DefaultUltimateSkillLevelLoss()
    {
        SetUltimateSkillLevelLoss(_defaultConfig.Ultimate.ChanceOfSkillLevelLoss);
    }


    private void SetUltimatePlayerLevelLoss(int value)
    {
        value = Math.Clamp(value, 0, 100);

        ConfigManager.RuntimeConfig.Ultimate.ChanceOfPlayerLevelLoss = value;

        UpdateInt(value, ConfigManager.OriginalConfig.Ultimate.ChanceOfPlayerLevelLoss, nameof(SetUltimatePlayerLevelLoss));
    }


    private void UndoUltimatePlayerLevelLoss()
    {
        SetUltimatePlayerLevelLoss(ConfigManager.OriginalConfig.Ultimate.ChanceOfPlayerLevelLoss);
    }


    private void DefaultUltimatePlayerLevelLoss()
    {
        SetUltimatePlayerLevelLoss(_defaultConfig.Ultimate.ChanceOfPlayerLevelLoss);
    }

    private void SetHardcoreSkillLevelLoss(int value)
    {
        value = Math.Clamp(value, 0, 100);

        ConfigManager.RuntimeConfig.Hardcore.ChanceOfSkillLevelLoss = value;

        UpdateInt(value, ConfigManager.OriginalConfig.Hardcore.ChanceOfSkillLevelLoss, nameof(SetHardcoreSkillLevelLoss));
    }


    private void UndoHardcoreSkillLevelLoss()
    {
        SetHardcoreSkillLevelLoss(ConfigManager.OriginalConfig.Hardcore.ChanceOfSkillLevelLoss);
    }


    private void DefaultHardcoreSkillLevelLoss()
    {
        SetHardcoreSkillLevelLoss(_defaultConfig.Hardcore.ChanceOfSkillLevelLoss);
    }


    private void SetHardcorePlayerLevelLoss(int value)
    {
        value = Math.Clamp(value, 0, 100);

        ConfigManager.RuntimeConfig.Hardcore.ChanceOfPlayerLevelLoss = value;

        UpdateInt(value, ConfigManager.OriginalConfig.Hardcore.ChanceOfPlayerLevelLoss, nameof(SetHardcorePlayerLevelLoss));
    }


    private void UndoHardcorePlayerLevelLoss()
    {
        SetHardcorePlayerLevelLoss(ConfigManager.OriginalConfig.Hardcore.ChanceOfPlayerLevelLoss);
    }


    private void DefaultHardcorePlayerLevelLoss()
    {
        SetHardcorePlayerLevelLoss(_defaultConfig.Hardcore.ChanceOfPlayerLevelLoss);
    }

    private void SetDebugLosses(bool value)
    {
        ConfigManager.RuntimeConfig.DebugSettings.DebugLosses = value;

        UpdateBool(value, ConfigManager.OriginalConfig.DebugSettings.DebugLosses, nameof(SetDebugLosses));
    }


    private void UndoDebugLosses()
    {
        SetDebugLosses(ConfigManager.OriginalConfig.DebugSettings.DebugLosses);
    }


    private void DefaultDebugLosses()
    {
        SetDebugLosses(_defaultConfig.DebugSettings.DebugLosses);
    }
}