using MoreBotsServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Mod;
using System.Reflection;

namespace MoreBotsServer;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.morebotsapi.tacticaltoaster";
    public override string Name { get; init; } = "MoreBotsAPI";
    public override string Author { get; init; } = "TacticalToaster";
    public override List<string>? Contributors { get; init; } = new() { };
    public override SemanticVersioning.Version Version { get; init; } = new(1, 0, 0);
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class MoreBotsLib(
    MoreBotsCustomBotTypeService customBotTypeService,
    MoreBotsCustomBotConfigService customBotConfigService
) : IOnLoad
{
    public Task OnLoad()
    {
        return Task.CompletedTask;
    }

    public async Task LoadBots(Assembly assembly)
    {
        await customBotTypeService.CreateCustomBotTypes(assembly);
        await customBotConfigService.LoadCustomBotConfigs(assembly);
    }
}