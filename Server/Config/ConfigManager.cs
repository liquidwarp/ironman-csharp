namespace IronManServer.Config;

using System.Reflection;
using Models;
using Models.Enums;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Utils;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.Preload)]
public class ConfigManager(
    ModHelper modHelper,
    JsonUtil jsonUtil,
    FileUtil fileUtil,
    ISptLogger<ConfigManager> logger,
    ApplyConfig applyConfig,
    IronManConfig runtimeConfig) : IOnLoad
{
    public IronManConfig RuntimeConfig { get; } = runtimeConfig;
    public IronManConfig OriginalConfig {get; private set;} = null!;
    public IronManConfig DefaultConfig { get; } = new();

    private int _isActivelyProcessingFlag;
    
    public event Action? OnChange;

    private void NotifyChanged() => OnChange?.Invoke();
    
    private string ConfigPath
    {
        get => Path.Combine(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "config.json");
    }
    
    public async Task OnLoadAsync(CancellationToken ct)
    {
        OriginalConfig = DeepClone(RuntimeConfig);
        
        await applyConfig.ApplyConfiguration(ct);
    }
    
    public async Task<ConfigOperationResult> ReloadConfig(CancellationToken ct = default)
    {
        if (Interlocked.CompareExchange(ref _isActivelyProcessingFlag, 1, 0) != 0)
            return ConfigOperationResult.ActiveProcess;

        try
        {
            var loadedConfig = await jsonUtil.DeserializeFromFileAsync<IronManConfig>(ConfigPath, ct);
            if (loadedConfig is null)
            {
                throw new InvalidOperationException("Config file deserialized to null.");
            }

            CopyConfig(loadedConfig, RuntimeConfig);
            OriginalConfig = DeepClone(RuntimeConfig);
            await applyConfig.ApplyConfiguration(ct);
            
            return ConfigOperationResult.Success;
        }
        catch (Exception ex)
        { 
            logger.Error($"Failed to reload config: {ex}");
            return ConfigOperationResult.Failure;
        }
        finally
        {
            Interlocked.Exchange(ref _isActivelyProcessingFlag, 0);
            NotifyChanged();
        }
    }
    
    public async Task<ConfigOperationResult> SaveConfig(CancellationToken ct = default)
    {
        if (Interlocked.CompareExchange(ref _isActivelyProcessingFlag, 1, 0) != 0)
            return ConfigOperationResult.ActiveProcess;

        try
        {
            var serializedConfig  = jsonUtil.Serialize(RuntimeConfig, true);
            if (serializedConfig is null)
            {
                throw new InvalidOperationException("Config file serialized null.");
            }

            await fileUtil.WriteFileAsync(ConfigPath, serializedConfig, ct);
            
            OriginalConfig = DeepClone(RuntimeConfig);
            await applyConfig.ApplyConfiguration(ct);
            
            return ConfigOperationResult.Success;
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to save config: {ex}");
            return ConfigOperationResult.Failure;
        }
        finally
        {
            Interlocked.Exchange(ref _isActivelyProcessingFlag, 0);
            NotifyChanged();
        }
    }
    
    public async Task<ConfigOperationResult> ResetConfig(CancellationToken ct = default)
    {
        if (Interlocked.CompareExchange(ref _isActivelyProcessingFlag, 1, 0) != 0)
            return ConfigOperationResult.ActiveProcess;

        try
        {
            var defaultConfig = new IronManConfig();

            CopyConfig(defaultConfig, RuntimeConfig);
            await SaveConfigInternal(ct);

            OriginalConfig = DeepClone(RuntimeConfig);
            await applyConfig.ApplyConfiguration(ct);

            return ConfigOperationResult.Success;
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to reset config: {ex}");
            return ConfigOperationResult.Failure;
        }
        finally
        {
            Interlocked.Exchange(ref _isActivelyProcessingFlag, 0);
            NotifyChanged();
        }
    }
    
    private async Task SaveConfigInternal(CancellationToken ct)
    {
        var serializedConfig = jsonUtil.Serialize(RuntimeConfig, true);
        if (serializedConfig is null)
        {
            throw new InvalidOperationException("Config file serialized null.");
        }
        await fileUtil.WriteFileAsync(ConfigPath, serializedConfig, ct);
    }

    private void CopyConfig(IronManConfig source, IronManConfig destination)
    {
        var clone = DeepClone(source);

        foreach (var property in typeof(IronManConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanWrite)
                continue;
            
            property.SetValue(destination, property.GetValue(clone));
        }
    }
    
    private T DeepClone<T>(T source)
    {
        var json = jsonUtil.Serialize(source);
        return jsonUtil.Deserialize<T>(json)!;
    }
}
