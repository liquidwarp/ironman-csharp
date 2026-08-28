namespace IronManServer.Config;

using System.Reflection;
using System.Text.Json;
using Models;
using SPTarkov.Server.Core.DI;

public class ConfigRegistration : IOnDIConstruct
{
    public static async Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken ct)
    {
        var config = await LoadConfigFromDiskAsync(ct);
        serviceCollection.AddSingleton(config);
    }

    private static async Task<IronManConfig> LoadConfigFromDiskAsync(CancellationToken ct)
    {
        var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), "config.json");

        IronManConfig config;

        if (!File.Exists(configPath))
        {
            config = new IronManConfig();
        }
        else
        {
            await using var stream = File.OpenRead(configPath);
            config = await JsonSerializer.DeserializeAsync<IronManConfig>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct) ?? new IronManConfig();
        }

        await SaveConfigToDiskAsync(config, configPath, ct);

        return config;
    }

    private static async Task SaveConfigToDiskAsync(IronManConfig config, string path, CancellationToken ct)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, config, new JsonSerializerOptions { WriteIndented = true }, ct);
    }
}
