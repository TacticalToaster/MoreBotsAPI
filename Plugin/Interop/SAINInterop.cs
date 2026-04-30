using DrakiaXYZ.BigBrain.Brains;
using EFT;
using HarmonyLib;
using SAIN.Attributes;
using SAIN.Preset;
using SAIN.Preset.BotSettings;
using SAIN.Preset.BotSettings.SAINSettings;
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
            //AddSAINLayers();
            CreateCustomBotTypes();
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
                    BaseBrain = setting.BaseBrain
                };
                
                BotTypeDefinitions.AddBotType(botType);
                
                botSettings.AddBotTypeToSettings(botType, setting.DifficultyModifier);
                

                Plugin.LogSource.LogInfo($"Added SAIN BotType: {botType.Name} with WildSpawnType {botType.WildSpawnType}");
            }
        }
    }
}
