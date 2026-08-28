namespace IronManServer.Models;

using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Spt.Mod;

public record CustomItemDefinition : NewItemDetailsBase
{
    [JsonPropertyName("newId")]
    public string NewId { get; set; } = string.Empty;

    [JsonPropertyName("parentId")]
    public string ParentId { get; set; } = string.Empty;

    [JsonPropertyName("newItemName")]
    public string NewItemName { get; set; } = string.Empty;

    [JsonPropertyName("itemTplToClone")]
    public string ItemTplToClone { get; set; } = string.Empty;

    [JsonPropertyName("grid")]
    public GridOverride? Grid { get; set; }
}

public record GridOverride
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("cellsH")]
    public int? CellsH { get; set; }

    [JsonPropertyName("cellsV")]
    public int? CellsV { get; set; }
}
