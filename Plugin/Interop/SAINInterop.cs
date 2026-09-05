using DrakiaXYZ.BigBrain.Brains;
using EFT;
using HarmonyLib;
using SAIN.Attributes;
using SAIN.Preset;
using SAIN.Preset.BotSettings;
using System;
using System.Collections.Generic;
using System.Reflection;
using SAIN;

namespace MoreBotsAPI.Interop
{
    public class SAINInterop
    {
        public void Init()
        {
            Plugin.LogSource.LogInfo("Initializing SAIN interop for MoreBotsAPI...");
            new Harmony("com.morebotsapi.sain-enum-compat").Patch(
                AccessTools.Method(typeof(SAIN.Extensions.SainEnumMirrorExtensions), "ToESain", new[] { typeof(WildSpawnType) }),
                prefix: new HarmonyMethod(typeof(SAINInterop), nameof(MapCustomRoleForSain)));
            //AddSAINLayers();
            // Upstream 2.1 registers settings through its server interop.
            // Retain the conversion guard for clients using SAIN's closed enum.
            //CreateCustomBotTypes();
        }

        // SAIN 4.5 keeps a separate, closed role enum. Use the role declared as
        // the custom bot's vanilla brain only at that conversion boundary.
        // The actual profile role remains custom for factions and BigBrain.
        internal static void MapCustomRoleForSain(ref WildSpawnType type)
        {
            if (CustomWildSpawnTypeManager.GetCustomWildSpawnTypeDict().TryGetValue((int)type, out var custom)
                && Enum.IsDefined(typeof(SAIN.Preset.Shared.Enums.ESainWildSpawnType), custom.BaseBrain))
                type = (WildSpawnType)custom.BaseBrain;
        }

        private static readonly string[] commonVanillaLayersToRemove = new string[]
        {
            "Help",
            "AdvAssaultTarget",
            "Hit",
            "Simple Target",
            "Pmc",
            "AssaultHaveEnemy",
            "Assault Building",
            "Enemy Building",
            "PushAndSup",
            "Pursuit",
        };

        public static void AddSAINLayers()
        {
            foreach (var setting in CustomWildSpawnTypeManager.GetSAINSettings())
            {
                var layers = new List<string>();
                layers.AddRange(commonVanillaLayersToRemove);

                if (setting.LayersToRemove != null)
                {
                    layers.AddRange(setting.LayersToRemove);
                }

                if (setting.BrainsToApply == null || setting.BrainsToApply.Count == 0)
                {
                    setting.BrainsToApply = new List<string>() { setting.BaseBrain };
                }

                var roleList = new List<WildSpawnType>() { (WildSpawnType)setting.WildSpawnType };
                
                BigBrainHandler.BrainAssignment.AddCustomLayersToBrainsAndRoles(setting.BrainsToApply, roleList, false);
                BigBrainHandler.BrainAssignment.ToggleVanillaLayersForBrainsAndRoles(setting.BrainsToApply, roleList, layers, false);
                
                //BrainManager.RemoveLayers(layers, setting.BrainsToApply, new List<WildSpawnType> { (WildSpawnType)setting.WildSpawnType });
            }
        }

        public static void CreateCustomBotTypes()
        {
            Plugin.LogSource.LogInfo("Creating custom bot types for SAIN...");

            var preset = SAINPresetClass.Instance;
            var botSettings = preset.BotSettings;

            foreach (var setting in CustomWildSpawnTypeManager.GetSAINSettings())
            {
                var botType = new BotType()
                {
                    Name = setting.Name,
                    Description = setting.Description,
                    Section = setting.Section,
                    WildSpawnType = (WildSpawnType)setting.WildSpawnType,
                    //BaseBrain = setting.BaseBrain
                };
                
                //BotTypeDefinitions.AddBotType(botType);
                
                //botSettings.AddBotTypeToSettings(botType, setting.DifficultyModifier);
                

                Plugin.LogSource.LogInfo($"Added SAIN BotType: {botType.Name} with WildSpawnType {botType.WildSpawnType}");
            }
        }
    }
}
