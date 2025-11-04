using MoreBotsServer.Models;
using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
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

    private void GetDatabaseTables()
    {
        if (_databaseTables == null) _databaseTables = databaseService.GetTables();
    }

    // Create custom bot types using your mod db folders.
    // Do note that types get added to the database fully lowercase. SPT requires it like that to work.
    // If you want to edit the type after it is created, make sure you account for the lowercase name when indexing the table.
    public async Task CreateCustomBotTypes(Assembly assembly, string? relativePath = null)
    {
        GetDatabaseTables();

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

                botTypeName = botTypeName.ToLowerInvariant();

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
        GetDatabaseTables();

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

            if (botTypeData == null)
            {
                logger.Warning($"Could not read {file} as bot type data! Skipping loading shared bot types.");
                return;
            }

            foreach (var botTypeName in botTypeNames)
            {
                botTypeData = await jsonUtil.DeserializeFromFileAsync<BotType>(file);
                var botTypeNameLower = botTypeName.ToLowerInvariant();
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

    // Replace individual settings of existing bot types using your mod db folders.
    // Difficulty settings will only replace the settings you provide, leaving others intact.
    // Other settings are less granular, typically replacing the entire section or having one layer of granularity.
    // This lets you modify existing bot types without needing to redefine the entire type, or create multiple similar types with minor changes.
    public async Task LoadBotTypeReplace(Assembly assembly, string replaceFileName, List<string> botTypeNames)
    {
        GetDatabaseTables();

        try
        {
            var assemblyLocation = modHelper.GetAbsolutePathToModFolder(assembly);
            var botTypeDir = System.IO.Path.Combine("db", "bots", "sharedTypes");
            var finalDir = System.IO.Path.Combine(assemblyLocation, botTypeDir);

            logger.Info($"Starting type settings replacement using shared bot type file: {replaceFileName}");

            if (!Directory.Exists(finalDir))
            {
                logger.Warning($"Directory for shared custom bot types not found at {finalDir}");
                return;
            }

            var files = Directory.GetFiles(finalDir, replaceFileName + ".json*");

            if (!files.Any())
            {
                logger.Warning($"Shared bot type file {replaceFileName} not found at {finalDir}");
                return;
            }

            var file = files[0];

            var botTypeData = await jsonUtil.DeserializeFromFileAsync<BotTypeReplace>(file);

            if (botTypeData == null)
            {
                logger.Warning($"Could not read {file} as bot type data! Skipping replacing bot types.");
                return;
            }

            foreach (var botTypeName in botTypeNames)
            {
                var botTypeNameLower = botTypeName.ToLowerInvariant();

                botTypeData = await jsonUtil.DeserializeFromFileAsync<BotTypeReplace>(file);

                ReplaceBotSettings(_databaseTables.Bots.Types[botTypeNameLower], botTypeData);

                logger.Info($"Successfully replaced settings in bot type: {botTypeNameLower}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error replacing settings in bot types: {ex.Message}");
        }
    }

    public async Task LoadBotTypeReplaceByTypes(Assembly assembly, List<string> botTypeNames)
    {
        GetDatabaseTables();

        try
        {
            var assemblyLocation = modHelper.GetAbsolutePathToModFolder(assembly);
            var botTypeDir = System.IO.Path.Combine("db", "bots", "sharedTypes");
            var finalDir = System.IO.Path.Combine(assemblyLocation, botTypeDir);

            logger.Info($"Starting type settings replacement using shared bot type files");

            if (!Directory.Exists(finalDir))
            {
                logger.Warning($"Directory for shared custom bot types not found at {finalDir}");
                return;
            }


            foreach (var botTypeName in botTypeNames)
            {
                var files = Directory.GetFiles(finalDir, botTypeName + ".json*");

                if (!files.Any())
                {
                    logger.Warning($"Shared bot type file {botTypeName} not found at {finalDir}");
                    continue;
                }

                var file = files[0];

                var botTypeData = await jsonUtil.DeserializeFromFileAsync<BotTypeReplace>(file);

                if (botTypeData == null)
                {
                    logger.Warning($"Could not read {file} as bot type data! Skipping replacing bot type {botTypeName}.");
                    continue;
                }

                var botTypeNameLower = botTypeName.ToLowerInvariant();

                botTypeData = await jsonUtil.DeserializeFromFileAsync<BotTypeReplace>(file);

                ReplaceBotSettings(_databaseTables.Bots.Types[botTypeNameLower], botTypeData);

                logger.Info($"Successfully replaced settings in bot type: {botTypeNameLower}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error replacing settings in bot types: {ex.Message}");
        }
    }

    public void ReplaceBotSettings(BotType typeToReplace, BotTypeReplace replacement)
    {
        if (replacement.BotAppearance != null) ReplaceBotAppearance(typeToReplace, replacement.BotAppearance);
        if (replacement.BotChances != null) ReplaceBotChances(typeToReplace, replacement.BotChances);
        if (replacement.BotDifficulty != null) ReplaceBotDifficulties(typeToReplace, replacement.BotDifficulty);
        if (replacement.BotExperience != null) ReplaceBotExperience(typeToReplace, replacement.BotExperience);
        if (replacement.FirstNames != null) ReplaceBotFirstNames(typeToReplace, replacement.FirstNames);
        if (replacement.LastNames != null) ReplaceBotLastNames(typeToReplace, replacement.LastNames);
        if (replacement.BotGeneration != null) ReplaceBotGeneration(typeToReplace, replacement.BotGeneration);
        if (replacement.BotHealth != null) ReplaceBotHealth(typeToReplace, replacement.BotHealth);
        if (replacement.BotInventory != null) ReplaceBotInventory(typeToReplace, replacement.BotInventory);
        if (replacement.BotSkills != null) ReplaceBotSkills(typeToReplace, replacement.BotSkills);
    }

    public void ReplaceBotAppearance(BotType typeToReplace, Appearance replacement)
    {
        if (replacement.Body != null) typeToReplace.BotAppearance.Body = replacement.Body;
        if (replacement.Feet != null) typeToReplace.BotAppearance.Feet = replacement.Feet;
        if (replacement.Hands != null) typeToReplace.BotAppearance.Hands = replacement.Hands;
        if (replacement.Head != null) typeToReplace.BotAppearance.Head = replacement.Head;
        if (replacement.Voice != null) typeToReplace.BotAppearance.Voice = replacement.Voice;
    }

    public void ReplaceBotChances(BotType typeToReplace, Chances replacement)
    {
        if (replacement.EquipmentChances != null) typeToReplace.BotChances.EquipmentChances = replacement.EquipmentChances;
        if (replacement.WeaponModsChances != null) typeToReplace.BotChances.WeaponModsChances = replacement.WeaponModsChances;
        if (replacement.EquipmentModsChances != null) typeToReplace.BotChances.EquipmentModsChances = replacement.EquipmentModsChances;
    }

    public void ReplaceBotDifficulties(BotType typeToReplace, Dictionary<string, DifficultyCategoriesReplace> replacement)
    {
        if (replacement.ContainsKey("all"))
        {
            foreach (var category in replacement["all"].GetAllPropertiesAsDictionary())
            {
                var replacementSettings = category.Value.GetAllPropertiesAsDictionary();
                foreach (var setting in replacementSettings)
                {
                    if (setting.Value == null) continue;
                    ReplaceBotDifficultySettings(typeToReplace, "easy", category.Key, setting.Key, setting.Value);
                    ReplaceBotDifficultySettings(typeToReplace, "normal", category.Key, setting.Key, setting.Value);
                    ReplaceBotDifficultySettings(typeToReplace, "hard", category.Key, setting.Key, setting.Value);
                    ReplaceBotDifficultySettings(typeToReplace, "impossible", category.Key, setting.Key, setting.Value);
                }
            }
        }

        if (replacement.ContainsKey("easy"))
            ReplaceBotDifficultyCategory(typeToReplace, "easy", replacement);

        if (replacement.ContainsKey("normal"))
            ReplaceBotDifficultyCategory(typeToReplace, "normal", replacement);

        if (replacement.ContainsKey("hard"))
            ReplaceBotDifficultyCategory(typeToReplace, "hard", replacement);

        if (replacement.ContainsKey("impossible"))
            ReplaceBotDifficultyCategory(typeToReplace, "impossible", replacement);
    }

    private void ReplaceBotDifficultyCategory(BotType typeToReplace, string difficulty, Dictionary<string, DifficultyCategoriesReplace> replacement)
    {
        foreach (var category in replacement[difficulty].GetAllPropertiesAsDictionary())
        {
            var replacementSettings = category.Value.GetAllPropertiesAsDictionary();
            foreach (var setting in replacementSettings)
            {
                if (setting.Value == null) continue;
                ReplaceBotDifficultySettings(typeToReplace, difficulty, category.Key, setting.Key, setting.Value);
            }
        }
    }

    private void ReplaceBotDifficultySettings(BotType typeToReplace, string difficulty, string category, string setting, object replaceValue)
    {
        var categoryObject = typeToReplace.BotDifficulty[difficulty].GetType().GetProperty(category)?.GetValue(typeToReplace.BotDifficulty[difficulty]);
        categoryObject?.GetType().GetProperty(setting)?.SetValue(categoryObject, replaceValue);
    }

    public void ReplaceBotExperience(BotType typeToReplace, Experience replacement)
    {
        if (replacement.AggressorBonus != null) typeToReplace.BotExperience.AggressorBonus = replacement.AggressorBonus;
        if (replacement.Level != null) typeToReplace.BotExperience.Level = replacement.Level;
        if (replacement.Reward != null) typeToReplace.BotExperience.Reward = replacement.Reward;
        if (replacement.StandingForKill != null) typeToReplace.BotExperience.StandingForKill = replacement.StandingForKill;
        if (replacement.UseSimpleAnimator != null) typeToReplace.BotExperience.UseSimpleAnimator = replacement.UseSimpleAnimator;
    }

    public void ReplaceBotFirstNames(BotType typeToReplace, List<string> replacement)
    {
        if (replacement != null) typeToReplace.FirstNames = replacement;
    }

    public void ReplaceBotLastNames(BotType typeToReplace, IEnumerable<string> replacement)
    {
        if (replacement != null) typeToReplace.LastNames = replacement;
    }

    public void ReplaceBotGeneration(BotType typeToReplace, Generation replacement)
    {
        if (replacement != null) typeToReplace.BotGeneration = replacement;
    }

    public void ReplaceBotHealth(BotType typeToReplace, BotTypeHealth replacement)
    {
        if (replacement != null) typeToReplace.BotHealth = replacement;
    }

    public void ReplaceBotInventory(BotType typeToReplace, BotTypeInventory replacement)
    {
        if (replacement.Equipment != null) typeToReplace.BotInventory.Equipment = replacement.Equipment;
        if (replacement.Ammo != null) typeToReplace.BotInventory.Ammo = replacement.Ammo;
        if (replacement.Items != null) typeToReplace.BotInventory.Items = replacement.Items;
        if (replacement.Mods != null) typeToReplace.BotInventory.Mods = replacement.Mods;
    }

    public void ReplaceBotSkills(BotType typeToReplace, BotDbSkills replacement)
    {
        if (replacement.Common != null) typeToReplace.BotSkills.Common = replacement.Common;
        if (replacement.Mastering != null) typeToReplace.BotSkills.Mastering = replacement.Mastering;
    }

    public Dictionary<string, Dictionary<string, DifficultyCategories>>? GetBotDifficulties(string url, EmptyRequestData info, string sessionID, string output)
    {
        GetDatabaseTables();

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