namespace IronManServer;

using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;

public record ModMetadata : IModMetadata, IModBlazorMetadata
{
    public string ModGuid { get; init; } = "com.acidphantasm.ironman";
    public string Name { get; init; } = "Ironman";
    public string Author { get; init; } = "acidphantasm";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("1.0.2");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.3");
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; } = "https://github.com/acidphantasm/ironman-csharp";
    public string License { get; init; } = "CC BY-NC-ND 4.0";
    public bool HasPrepatcher { get; init; } = false;
    public string? WWWRootUrl { get; init; }
    public string? HomePage { get; init; } = "/ironman";
    public string? HomePageDescription { get; init; } = "Ironman information, profile rules, profile status, and configuration settings";
}
