

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
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
    public List<string> LoadedBotTypes { get; } = new();

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

                botTypeName = botTypeName.ToLower();

                logger.Info($"Loading custom bot type: {botTypeName}");

                if (botTypeData == null)
                {
                    logger.Warning($"Could not read {file} as bot type data! Skipping.");
                    continue;
                }

                _databaseTables.Bots.Types[botTypeName] = botTypeData;
                LoadedBotTypes.Add(botTypeName);

                logger.Info($"Successfully loaded custom bot type: {botTypeName}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error loading custom bot types: {ex.Message}");
        }
    }

    public async Task CreateCustomBotTypesShared(Assembly assembly, string sharedFileName, List<string> botTypeNames)
    {
        if (_databaseTables == null) _databaseTables = databaseService.GetTables();

        try
        {
            var assemblyLocation = modHelper.GetAbsolutePathToModFolder(assembly);
            var botTypeDir = System.IO.Path.Combine("db", "bots", "sharedTypes");
            var finalDir = System.IO.Path.Combine(assemblyLocation, botTypeDir);

            if (!Directory.Exists(finalDir))
            {
                logger.Warning($"Directory for shared custom bot types not found at {finalDir}");
                return;
            }

            var files = Directory.GetFiles(finalDir, sharedFileName + ".json*");

            if (!files.Any())
            {
                logger.Warning($"Shared bot type file {sharedFileName} not found at {finalDir}");
                return;
            }

            var file = files[0];

            var botTypeData = await jsonUtil.DeserializeFromFileAsync<BotType>(file);

            if (botTypeData != null)
            {
                logger.Warning($"Could not read {file} as bot type data! Skipping loading shared bot types.");
                return;
            }

            foreach (var botTypeName in botTypeNames)
            {
                var botTypeNameLower = botTypeName.ToLower();
                _databaseTables.Bots.Types[botTypeNameLower] = botTypeData;
                LoadedBotTypes.Add(botTypeNameLower);

                logger.Info($"Successfully loaded shared custom bot type: {botTypeNameLower}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error loading shared custom bot types: {ex.Message}");
        }
    }

    public Dictionary<string, Dictionary<string, DifficultyCategories>>? GetBotDifficulties(string url, EmptyRequestData info, string sessionID, string output)
    {
        if (_databaseTables == null) _databaseTables = databaseService.GetTables();

        try
        {
            var botDifficulties = _databaseTables.Bots.Types;

            Dictionary<string, Dictionary<string, DifficultyCategories>> result = new();

            if (output != null && output != string.Empty)
            {
                result = jsonUtil.Deserialize<Dictionary<string, Dictionary<string, DifficultyCategories>>>(output);
            }

            if (botDifficulties == null || !botDifficulties.Any())
            {
                logger.Warning("Bot difficulties data is missing or empty.");
                return null;
            }

            foreach (var botType in LoadedBotTypes)
            {
                logger.Info($"Processing bot type: {botType}");

                var botData = botDifficulties.ContainsKey(botType) ? botDifficulties[botType] : null;

                if (!result.ContainsKey(botType))
                {
                    result[botType] = new Dictionary<string, DifficultyCategories>();
                }

                result[botType]["easy"] = botData.BotDifficulty["easy"];
                result[botType]["normal"] = botData.BotDifficulty["normal"];
                result[botType]["hard"] = botData.BotDifficulty["hard"];
                result[botType]["impossible"] = botData.BotDifficulty["impossible"];
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.Error($"Error retrieving custom bot difficulties: {ex.Message}");
            return null;
        }
    }
}