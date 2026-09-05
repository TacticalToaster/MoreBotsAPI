using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.Profile;
using MoreBotsServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Reflection;
using MoreBotsServer.Models;

namespace MoreBotsServer;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.morebotsapi.tacticaltoaster";
    public string Name { get; init; } = "MoreBotsAPI";
    public string Author { get; init; } = "TacticalToaster";
    public List<string>? Contributors { get; init; } = new() { };
    public SemanticVersioning.Version Version { get; init; } = new(2, 0, 1);
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.3");
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public bool? IsBundleMod { get; init; }
    public bool HasPrepatcher { get; init; } = false;
    public string License { get; init; } = "MIT";
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

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.Preload + 5)]
public class MoreBotsAPI(
    MoreBotsCustomBotTypeService customBotTypeService,
    MoreBotsCustomBotConfigService customBotConfigService,
    ConfigService configService,
    BotConfig botConfig
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        if (configService.ModConfig.increaseBotCapAmount > 0)
        {
            var botCaps = botConfig.MaxBotCap;

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
                    output, cancellationToken
                ) => {
                    var result = _customBotTypeService.GetBotDifficulties(url, (EmptyRequestData)info, sessionID, output);
                    return await new ValueTask<string>(_httpResponseUtil.NoBody(result));
                }
            )
        ];
    }
}

[Injectable]
public class MoreBotsGetFactionsRouter : StaticRouter
{
    private static HttpResponseUtil _httpResponseUtil;
    private static FactionService _factionService;

    public MoreBotsGetFactionsRouter(
        FactionService factionService,
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil) : base(jsonUtil, GetCustomRoutes())
    {
        _httpResponseUtil = httpResponseUtil;
        _factionService = factionService;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction(
                "/morebotsapi/getfactions",
                async (
                    url,
                    info,
                    sessionID,
                    output, cancellationToken
                ) => {
                    return await new ValueTask<string>(_httpResponseUtil.NoBody(_factionService.GetAllFactions()));
                }
            )
        ];
    }
}

[Injectable]
public class MoreBotsFactionUpdateRevengeRouter : StaticRouter
{
    private static HttpResponseUtil _httpResponseUtil;
    private static FactionService _factionService;

    public MoreBotsFactionUpdateRevengeRouter(
        FactionService factionService,
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil) : base(jsonUtil, GetCustomRoutes())
    {
        _httpResponseUtil = httpResponseUtil;
        _factionService = factionService;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction<UpdateRevengeRequest>(
                "/morebotsapi/updaterevenge",
                async (
                    url,
                    info,
                    sessionID,
                    output, cancellationToken
                ) => {
                    await _factionService.AdjustFactionRevengeAsync(info, cancellationToken);
                    return await new ValueTask<string>(string.Empty);
                }
            )
        ];
    }
}

[Injectable]
public class MoreBotsFactionGetRevengesRouter : StaticRouter
{
    private static HttpResponseUtil _httpResponseUtil;
    private static FactionService _factionService;

    public MoreBotsFactionGetRevengesRouter(
        FactionService factionService,
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil) : base(jsonUtil, GetCustomRoutes())
    {
        _httpResponseUtil = httpResponseUtil;
        _factionService = factionService;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction(
                "/morebotsapi/getrevenges",
                async (
                    url,
                    info,
                    sessionID,
                    output, cancellationToken
                ) => {
                    return await new ValueTask<string>(_httpResponseUtil.NoBody(await _factionService.GetFactionsRevengesAsync(cancellationToken)));
                }
            )
        ];
    }
}