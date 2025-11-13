using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT;
using HarmonyLib;
using MoreBotsAPI.Components;
using MoreBotsAPI.Patches;
using Newtonsoft.Json;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MoreBotsAPI
{
    [BepInDependency("xyz.drakia.bigbrain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("me.sol.sain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ClientInfo.GUID, ClientInfo.PluginName, ClientInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        public static ConfigEntry<bool> DrawBotZones;

        public static List<BotZone> BotZones;

        public static string pluginPath = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "MoreBotsAPI");

        // BaseUnityPlugin inherits MonoBehaviour, so you can use base unity functions like Awake() and Update()
        private void Awake()
        {
            // save the Logger to variable so we can use it elsewhere in the project
            LogSource = Logger;

            FieldInfo excludedDifficultiesField = typeof(LocalBotSettingsProviderClass).GetField("Dictionary_1", BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidOperationException("ExcludedDifficulties field not found.");
            var excludedDifficulties = (Dictionary<WildSpawnType, List<BotDifficulty>>)excludedDifficultiesField.GetValue(null);

            var defaultExcludedDifficulties = new List<BotDifficulty>
            {
                BotDifficulty.easy,
                BotDifficulty.hard,
                BotDifficulty.impossible
            };

            foreach (var botType in CustomWildSpawnTypeManager.GetCustomWildSpawnTypes())
            {
                if (!excludedDifficulties.ContainsKey((WildSpawnType)botType.WildSpawnTypeValue))
                {
                    if (botType.ExcludedDifficulties != null)
                        excludedDifficulties.Add((WildSpawnType)botType.WildSpawnTypeValue, botType.ExcludedDifficulties.ConvertAll(difficultyInt => (BotDifficulty)difficultyInt));
                    else
                        excludedDifficulties.Add((WildSpawnType)botType.WildSpawnTypeValue, defaultExcludedDifficulties);

                    Logger.LogInfo($"Successfully added {botType.WildSpawnTypeName} : {botType.WildSpawnTypeValue} to the excluded difficulties list");
                }
                Traverse.Create(typeof(BotSettingsRepoClass)).Field<Dictionary<WildSpawnType, GClass790>>("Dictionary_0").Value.Add((WildSpawnType)botType.WildSpawnTypeValue, new GClass790(botType.IsBoss, botType.IsFollower, botType.IsHostileToEverybody, $"ScavRole/{botType.ScavRole}", (ETagStatus)0));

                if (botType.CountAsBossForStatistics.HasValue)
                {
                    BotSettingsRepoClass.Dictionary_0[(WildSpawnType)botType.WildSpawnTypeValue].CountAsBossForStatistics = botType.CountAsBossForStatistics.Value;
                }
            }

            new TarkovInitPatch().Enable(); //For Sain stuff
            new FixRaidEndSpawnTypePatch().Enable();
            new StandartBotBrainActivatePatch().Enable();
            new SuitableFollowersListPatch().Enable();
            new FenceLoyaltyWarnPatch().Enable();
            new NewGamePatch().Enable();

            InitConfig();

            int oldWildSpawnTypeConverter = Array.FindIndex<JsonConverter>(JsonSerializerSettingsClass.Converters, c => c.GetType() == typeof(GClass1866<WildSpawnType>));
            LogSource.LogInfo($"Old WildSpawnTypeFromInt converter index: {oldWildSpawnTypeConverter} {JsonSerializerSettingsClass.Converters[oldWildSpawnTypeConverter]}");
            JsonSerializerSettingsClass.Converters[oldWildSpawnTypeConverter] = new WildSpawnTypeFromIntConverter<WildSpawnType>(true);
        }

        private void InitConfig()
        {
            DrawBotZones = Config.Bind(
                "Bot Zones",
                "Draw Bot Zones",
                false,
                "Draw Bot Zones"
                );

            DrawBotZones.SettingChanged += OnDrawBotZones;
        }

        private void OnDrawBotZones(object sender, EventArgs e)
        {
            if (DrawBotZones.Value)
            {
                ZoneDebugComponent.Enable();
            }
            else
            {
                ZoneDebugComponent.Disable();
            }
        }
    }

    internal class NewGamePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));

        [PatchPrefix]
        public static void PatchPrefix()
        {
            ZoneDebugComponent.Enable();
        }
    }
}
