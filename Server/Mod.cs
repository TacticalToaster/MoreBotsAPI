using MoreBotsServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Reflection;

namespace MoreBotsServer;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.morebotsapi.tacticaltoaster";
    public override string Name { get; init; } = "MoreBotsAPI";
    public override string Author { get; init; } = "TacticalToaster";
    public override List<string>? Contributors { get; init; } = new() { };
    public override SemanticVersioning.Version Version { get; init; } = new(1, 1, 0);
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

[Injectable(InjectionType.Singleton)]
public class MoreBotsLogger
{
    private readonly bool _enableLogs;
    private readonly ISptLogger<MoreBotsLogger> _logger;
    public MoreBotsLogger(
        ISptLogger<MoreBotsLogger> logger,
        ConfigService configService)
    {
        _enableLogs = configService.ModConfig.enableDebugLogs;
        _logger = logger;
    }

    public void Info(string message)
    {
        if (_enableLogs)
        {
            _logger.Info($"[MoreBotsAPI] {message}");
        }
    }
    public void Warning(string message)
    {
        if (_enableLogs)
        {
            _logger.Warning($"[MoreBotsAPI] WARNING: {message}");
        }
    }
    public void Error(string message)
    {
        _logger.Error($"[MoreBotsAPI] ERROR: {message}");
    }
}

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.PostDBModLoader + 5)]
public class MoreBotsAPI(
    MoreBotsCustomBotTypeService customBotTypeService,
    MoreBotsCustomBotConfigService customBotConfigService,
    ConfigService configService,
    ConfigServer configServer
) : IOnLoad
{
    public Task OnLoad()
    {
        if (configService.ModConfig.increaseBotCapAmount > 0)
        {
            var botCaps = configServer.GetConfig<BotConfig>().MaxBotCap;

            foreach (var map in botCaps.Keys)
            {
                botCaps[map] = botCaps[map] + configService.ModConfig.increaseBotCapAmount;
            }
        }

        return Task.CompletedTask;
    }

    public async Task LoadBots(Assembly assembly)
    {
        await customBotTypeService.CreateCustomBotTypes(assembly);
        await customBotConfigService.LoadCustomBotConfigs(assembly);
    }

    public async Task LoadBotsShared(Assembly assembly, string sharedFileName, List<string> botTypeNames)
    {
        await customBotTypeService.CreateCustomBotTypesShared(assembly, sharedFileName, botTypeNames);
        await customBotConfigService.LoadCustomBotConfigsShared(assembly, sharedFileName, botTypeNames);
    }
}

[Injectable]
public class MoreBotsSettingsRouter : DynamicRouter
{
    private static HttpResponseUtil _httpResponseUtil;
    private static MoreBotsCustomBotTypeService _customBotTypeService;

    public MoreBotsSettingsRouter(
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil,
        MoreBotsCustomBotTypeService customBotTypeService) : base(jsonUtil, GetRoutes())
    {
        _httpResponseUtil = httpResponseUtil;
        _customBotTypeService = customBotTypeService;
    }

    private static List<RouteAction> GetRoutes()
    {
        return [
            new RouteAction(
                "/singleplayer/settings/bot/difficulties",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => {
                    var result = _customBotTypeService.GetBotDifficulties(url, (EmptyRequestData)info, sessionID, output);
                    return await new ValueTask<string>(_httpResponseUtil.NoBody(result));
                }
            )
        ];
    }
}