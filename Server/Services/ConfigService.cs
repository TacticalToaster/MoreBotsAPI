using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Server;
using System.Reflection;

namespace MoreBotsServer;

[Injectable(InjectionType.Singleton)]
public class ConfigService
{
    public MainConfig ModConfig;

    public readonly ModHelper _modHelper;

    public ConfigService(ModHelper modHelper)
    {
        _modHelper = modHelper;

        var pathToMod = _modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        ModConfig = _modHelper.GetJsonDataFromFile<MainConfig>(pathToMod, "config.jsonc");
    }
}