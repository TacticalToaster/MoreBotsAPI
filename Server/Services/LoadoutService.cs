using MoreBotsServer.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Reflection;

namespace MoreBotsServer.Services;

[Injectable(InjectionType.Singleton)]
public class LoadoutService(
    ISptLogger<LoadoutService> logger,
    ModHelper modHelper,
    JsonUtil jsonUtil,
    DatabaseService databaseService,
    ItemHelper itemHelper
)
{
    public async Task LoadLoadoutsWithTemplate(Assembly assembly, string template)
    {
        try
        {
            var pathToMod = modHelper.GetAbsolutePathToModFolder(assembly);
            var loadoutDir = System.IO.Path.Combine(pathToMod, "db", "bots", "loadouts");

            if (!Directory.Exists(loadoutDir))
            {
                logger.Error($"Directory for custom loadouts not found at {loadoutDir}");
                return;
            }

            var files = Directory.GetFiles(loadoutDir, "*.json*");

            foreach (var file in files)
            {
                var loadout = await jsonUtil.DeserializeFromFileAsync<LoadoutInfo>(file);
                var botTypeName = System.IO.Path.GetFileNameWithoutExtension(file);

                botTypeName = botTypeName.ToLowerInvariant();

                logger.Info($"Loading loadout for custom bot type: {botTypeName}");

                if (loadout == null)
                {
                    logger.Warning($"Could not read {file} as bot loadout data! Skipping.");
                    continue;
                }

                ProcessLoadouts(assembly, botTypeName, databaseService.GetTables().Bots.Types[botTypeName], UseTemplateLoadout(assembly, template, loadout));

                logger.Info($"Successfully loaded custom loadout data for bot type: {botTypeName}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error loading custom loadouts with template: {ex.Message}");
        }
    }

    public LoadoutInfo UseTemplateLoadout(Assembly assembly, string template, LoadoutInfo loadout)
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(assembly);
        var commonDir = System.IO.Path.Combine(pathToMod, "db", "bots", "loadouts", "template");

        var combinedLoadout = new LoadoutInfo
        {
            Equipment = new Dictionary<string, Dictionary<string, LoadoutItem>>(),
            Weapons = new Dictionary<string, Dictionary<string, List<string>>>(),
            Categories = new Dictionary<string, Dictionary<string, LoadoutItem>>()
        };

        combinedLoadout = DeepMerge(combinedLoadout, loadout);

        if (!Directory.Exists(commonDir))
        {
            logger.Warning($"Template loadouts directory not found: {commonDir}");
            return combinedLoadout;
        }

        var files = Directory.GetFiles(commonDir, template + ".json*");

        if (files == null || files.Length == 0)
        {
            logger.Warning($"No template file found: {template}");
            return loadout;
        }

        var file = files[0];
        var templateLoadout = jsonUtil.Deserialize<LoadoutInfo>(File.ReadAllText(file));

        if (templateLoadout == null)
        {
            logger.Warning($"Could not read {template} as LoadoutInfo!");
            return loadout;
        }

        return DeepMerge(templateLoadout, combinedLoadout);
    }

    public void ProcessLoadouts(Assembly assembly, string type, BotType botData, LoadoutInfo baseLoadout)
    {
        var commonLoadout = LoadCommonLoadouts(assembly);
        var combinedInfo = DeepMerge(commonLoadout, baseLoadout);

        botData.BotInventory.Mods = new Dictionary<MongoId, Dictionary<string, HashSet<MongoId>>>();

        foreach (var slot in combinedInfo.Equipment.Keys)
        {
            var slotData = combinedInfo.Equipment[slot];
            foreach (var item in slotData.Keys)
            {
                var itemData = slotData[item];

                if (Enum.TryParse<EquipmentSlots>(slot, out var equipmentSlot))
                {
                    if (!botData.BotInventory.Equipment.ContainsKey(equipmentSlot))
                    {
                        botData.BotInventory.Equipment[equipmentSlot] = new Dictionary<MongoId, double>();
                    }
                    botData.BotInventory.Equipment[equipmentSlot][itemData.Id] = itemData.Chance ?? 0;
                }

                //botData.BotInventory.Equipment[slot][itemData.Id] = itemData.Chance ?? 100;

                var children = new Dictionary<string, LoadoutItem>();
                ProcessChildren(children, itemData, combinedInfo, type);
                ProcessItem(itemData.Id, itemData.Slots, type, combinedInfo, children);
            }
        }
    }

    private void AddModsToSlot(string item, string slot, List<string> mods, string type)
    {
        var dbTables = databaseService.GetTables();
        if (!dbTables.Bots.Types[type].BotInventory.Mods.ContainsKey(item))
        {
            dbTables.Bots.Types[type].BotInventory.Mods[item] = new Dictionary<string, HashSet<MongoId>>();
        }

        if (!dbTables.Bots.Types[type].BotInventory.Mods[item].ContainsKey(slot))
        {
            dbTables.Bots.Types[type].BotInventory.Mods[item][slot] = new HashSet<MongoId>();
        }

        foreach (var mod in mods)
        {
            if (!dbTables.Bots.Types[type].BotInventory.Mods[item][slot].Contains(mod))
            {
                dbTables.Bots.Types[type].BotInventory.Mods[item][slot].Add(mod);
            }
        }
    }

    private void ProcessItem(string item, Dictionary<string, List<string>> slots, string type, LoadoutInfo loadoutInfo, Dictionary<string, LoadoutItem> children = null)
    {
        children ??= new Dictionary<string, LoadoutItem>();

        if (slots == null) return;

        foreach (var slot in slots.Keys)
        {
            var categories = slots[slot];
            foreach (var category in categories)
            {
                var itemsInSlot = ProcessCategoryOrItem(item, category, type, loadoutInfo, children);
                AddModsToSlot(item, slot, itemsInSlot, type);
            }
        }
    }

    private List<string> ProcessCategoryOrItem(string item, string categoryOrItem, string type, LoadoutInfo loadoutInfo, Dictionary<string, LoadoutItem> children = null)
    {
        if (children != null && children.ContainsKey(categoryOrItem))
        {
            return ProcessCategoryOrItem(item, children[categoryOrItem].Id, type, loadoutInfo, children);
        }
        else if (loadoutInfo.Categories.ContainsKey(categoryOrItem))
        {
            return ProcessCategory(item, categoryOrItem, loadoutInfo, type, children);
        }
        else if (MongoId.IsValidMongoId(categoryOrItem) && itemHelper.IsItemInDb(categoryOrItem))
        {
            return new List<string> { categoryOrItem };
        }

        return new List<string>();
    }

    private List<string> ProcessCategory(string item, string category, LoadoutInfo loadoutInfo, string type, Dictionary<string, LoadoutItem> children = null)
    {
        children ??= new Dictionary<string, LoadoutItem>();

        var allItems = new List<string>();
        var categoryData = loadoutInfo.Categories[category];

        foreach (var categoryItem in categoryData.Keys)
        {
            var itemData = categoryData[categoryItem];

            ProcessChildren(children, itemData, loadoutInfo, type);
            allItems.Add(itemData.Id);

            if (itemData.Slots != null)
            {
                ProcessItem(itemData.Id, itemData.Slots, type, loadoutInfo, itemData.Children);
            }
        }

        return allItems;
    }

    private void ProcessChildren(Dictionary<string, LoadoutItem> children, LoadoutItem parent, LoadoutInfo loadoutInfo, string type)
    {
        if (parent.Children != null)
        {
            foreach (var child in parent.Children.Keys)
            {
                var childData = parent.Children[child];
                children[child] = childData;
                ProcessChildren(children, childData, loadoutInfo, type);

                if (childData.Slots != null)
                {
                    ProcessItem(childData.Id, childData.Slots, type, loadoutInfo, children);
                }
            }
        }
    }

    private LoadoutInfo LoadCommonLoadouts(Assembly assembly)
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(assembly);
        var commonDir = System.IO.Path.Combine(pathToMod, "db", "bots", "loadouts", "common");
        var combinedLoadout = new LoadoutInfo
        {
            Equipment = new Dictionary<string, Dictionary<string, LoadoutItem>>(),
            Weapons = new Dictionary<string, Dictionary<string, List<string>>>(),
            Categories = new Dictionary<string, Dictionary<string, LoadoutItem>>()
        };

        if (!Directory.Exists(commonDir))
        {
            logger.Warning($"Common loadouts directory not found: {commonDir}");
            return combinedLoadout;
        }

        var files = Directory.GetFiles(commonDir, "*.json*");
        logger.Info($"Found {files.Length} common loadout file(s) to merge");

        foreach (var file in files)
        {
            try
            {
                var fileContent = File.ReadAllText(file);
                var loadoutData = jsonUtil.Deserialize<LoadoutInfo>(fileContent);

                logger.Info($"Preparing merge of: {System.IO.Path.GetFileName(file)}");

                combinedLoadout = DeepMerge(combinedLoadout, loadoutData);
                logger.Info($"Merged common loadout file: {System.IO.Path.GetFileName(file)}");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load common loadout file {System.IO.Path.GetFileName(file)}: {ex.Message}");
            }
        }

        return combinedLoadout;
    }

    public LoadoutInfo DeepMerge(LoadoutInfo target, LoadoutInfo source)
    {
        if (target == null) return source;
        if (source == null) return target;

        // Merge Equipment
        foreach (var equipmentKey in source.Equipment.Keys)
        {
            if (!target.Equipment.ContainsKey(equipmentKey))
            {
                target.Equipment[equipmentKey] = new Dictionary<string, LoadoutItem>();
            }

            foreach (var itemKey in source.Equipment[equipmentKey].Keys)
            {
                if (target.Equipment[equipmentKey].ContainsKey(itemKey))
                {
                    target.Equipment[equipmentKey][itemKey] = DeepMergeLoadoutItem(
                        target.Equipment[equipmentKey][itemKey],
                        source.Equipment[equipmentKey][itemKey]
                    );
                }
                else
                {
                    target.Equipment[equipmentKey][itemKey] = source.Equipment[equipmentKey][itemKey];
                }
            }
        }

        // Merge Weapons
        foreach (var weaponKey in source.Weapons.Keys)
        {
            if (!target.Weapons.ContainsKey(weaponKey))
            {
                target.Weapons[weaponKey] = new Dictionary<string, List<string>>();
            }

            foreach (var weaponItemKey in source.Weapons[weaponKey].Keys)
            {
                if (target.Weapons[weaponKey].ContainsKey(weaponItemKey))
                {
                    target.Weapons[weaponKey][weaponItemKey].AddRange(
                        source.Weapons[weaponKey][weaponItemKey].Except(target.Weapons[weaponKey][weaponItemKey])
                    );
                }
                else
                {
                    target.Weapons[weaponKey][weaponItemKey] = new List<string>(source.Weapons[weaponKey][weaponItemKey]);
                }
            }
        }

        // Merge Categories
        foreach (var categoryKey in source.Categories.Keys)
        {
            if (!target.Categories.ContainsKey(categoryKey))
            {
                target.Categories[categoryKey] = new Dictionary<string, LoadoutItem>();
            }

            foreach (var categoryItemKey in source.Categories[categoryKey].Keys)
            {
                if (target.Categories[categoryKey].ContainsKey(categoryItemKey))
                {
                    target.Categories[categoryKey][categoryItemKey] = DeepMergeLoadoutItem(
                        target.Categories[categoryKey][categoryItemKey],
                        source.Categories[categoryKey][categoryItemKey]
                    );
                }
                else
                {
                    target.Categories[categoryKey][categoryItemKey] = source.Categories[categoryKey][categoryItemKey];
                }
            }
        }

        return target;
    }

    private LoadoutItem DeepMergeLoadoutItem(LoadoutItem target, LoadoutItem source)
    {
        if (target == null) return source;
        if (source == null) return target;

        // Merge properties
        target.Chance = source.Chance ?? target.Chance;

        // Merge Children
        if (source.Children != null)
        {
            if (target.Children == null)
            {
                target.Children = new Dictionary<string, LoadoutItem>();
            }

            foreach (var childKey in source.Children.Keys)
            {
                if (target.Children.ContainsKey(childKey))
                {
                    target.Children[childKey] = DeepMergeLoadoutItem(target.Children[childKey], source.Children[childKey]);
                }
                else
                {
                    target.Children[childKey] = source.Children[childKey];
                }
            }
        }

        // Merge Slots
        if (source.Slots != null)
        {
            if (target.Slots == null)
            {
                target.Slots = new Dictionary<string, List<string>>();
            }

            foreach (var slotKey in source.Slots.Keys)
            {
                if (target.Slots.ContainsKey(slotKey))
                {
                    target.Slots[slotKey].AddRange(source.Slots[slotKey].Except(target.Slots[slotKey]));
                }
                else
                {
                    target.Slots[slotKey] = new List<string>(source.Slots[slotKey]);
                }
            }
        }

        return target;
    }
}