using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json.Converters;
using System.Text.Json.Serialization;

namespace MoreBotsServer.Models;

public record BotTypeReplace
{
    [JsonPropertyName("appearance")]
    public Appearance? BotAppearance { get; set; }

    [JsonPropertyName("chances")]
    public Chances? BotChances { get; set; }

    [JsonPropertyName("difficulty")]
    public Dictionary<string, DifficultyCategoriesReplace>? BotDifficulty { get; set; }

    [JsonPropertyName("experience")]
    public Experience? BotExperience { get; set; }

    [JsonPropertyName("firstName")]
    public List<string>? FirstNames { get; set; }

    [JsonPropertyName("generation")]
    public Generation? BotGeneration { get; set; }

    [JsonPropertyName("health")]
    public BotTypeHealth? BotHealth { get; set; }

    [JsonPropertyName("inventory")]
    public BotTypeInventory? BotInventory { get; set; }

    [JsonPropertyName("lastName")]
    public IEnumerable<string>? LastNames { get; set; }

    [JsonPropertyName("skills")]
    public BotDbSkills? BotSkills { get; set; }
}

/// <summary>
/// See BotSettingsComponents in the client, this record should match that
/// </summary>
public record DifficultyCategoriesReplace
{
    public BotGlobalAimingSettings? Aiming { get; set; }

    public BotGlobalsBossSettings? Boss { get; set; }

    public BotGlobalsChangeSettings? Change { get; set; }

    public BotGlobalCoreSettings? Core { get; set; }

    public BotGlobalsCoverSettings? Cover { get; set; }

    public BotGlobalsGrenadeSettings? Grenade { get; set; }

    public BotGlobalsHearingSettings? Hearing { get; set; }

    public BotGlobalLayData? Lay { get; set; }

    public BotGlobalLookData? Look { get; set; }

    public BotGlobalsMindSettings? Mind { get; set; }

    public BotGlobalsMoveSettings? Move { get; set; }

    public BotGlobalPatrolSettings? Patrol { get; set; }

    public BotGlobalsScatteringSettings? Scattering { get; set; }

    public BotGlobalShootData? Shoot { get; set; }
}