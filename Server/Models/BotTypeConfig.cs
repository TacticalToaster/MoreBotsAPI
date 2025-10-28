

using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using System.Text.Json.Serialization;

namespace MoreBotsServer.Models;

public record BotTypeConfig
{
    [JsonPropertyName("type")]
    public string? BotType { get; set; }
    [JsonPropertyName("presetBatch")]
    public int? PresetBatch { get; set; }
    [JsonPropertyName("isBoss")]
    public bool? IsBoss { get; set; }
    [JsonPropertyName("durability")]
    public DefaultDurability? Durability { get; set; }
    [JsonPropertyName("itemSpawnLimits")]
    public Dictionary<MongoId, double>? ItemSpawnLimits { get; set; }
    [JsonPropertyName("equipment")]
    public EquipmentFilters? EquipmentFilters { get; set; }
    [JsonPropertyName("currencyStackSize")]
    public Dictionary<string, Dictionary<string, double>>? CurrencyStackSize { get; set; }
    [JsonPropertyName("mustHaveUniqueName")]
    public bool? MustHaveUniqueName { get; set; }
}