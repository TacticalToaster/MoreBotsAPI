

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Reflection;

namespace MoreBotsServer.Services;

[Injectable(InjectionType.Singleton)]
public class MoreBotsCustomBotTypeService(
    ISptLogger<MoreBotsCustomBotTypeService> logger,
    ModHelper modHelper,
    JsonUtil jsonUtil,
    DatabaseService databaseService
)
{
    private DatabaseTables? _databaseTables;

    public async Task CreateCustomBotTypes(Assembly assembly, string? relativePath = null)
    {
        if (_databaseTables == null) _databaseTables = databaseService.GetTables();

        try
        {
            var assemblyLocation = modHelper.GetAbsolutePathToModFolder(assembly);
            var botTypeDir = System.IO.Path.Combine("db", "bots", "types");
            var finalDir = System.IO.Path.Combine(assemblyLocation, botTypeDir);

            if (!Directory.Exists(finalDir))
            {
                logger.Error($"Directory for custom bot types not found at {finalDir}");
                return;
            }

            var files = Directory.GetFiles(finalDir, "*.json*");

            foreach (var file in files)
            {
                var botTypeData = await jsonUtil.DeserializeFromFileAsync<BotType>(file);
                var botTypeName = System.IO.Path.GetFileNameWithoutExtension(file);

                if (botTypeData != null)
                {
                    logger.Warning($"Could not read {file} as bot type data! Skipping.");
                    continue;
                }

                _databaseTables.Bots.Types[botTypeName] = botTypeData;
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error loading custom bot types: {ex.Message}");
        }
    }
}