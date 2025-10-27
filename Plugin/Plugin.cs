using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using EFT;
using HarmonyLib;
using MoreBotsAPI.Interop;
using MoreBotsAPI.Patches;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MoreBotsAPI
{
    [BepInDependency("xyz.drakia.bigbrain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("me.sol.sain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ClientInfo.GUID, ClientInfo.PluginName, ClientInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

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

                    Logger.LogInfo($"Successfully added {botType.WildSpawnTypeName} to the excluded difficulties list");
                }
                Traverse.Create(typeof(BotSettingsRepoClass)).Field<Dictionary<WildSpawnType, GClass790>>("Dictionary_0").Value.Add((WildSpawnType)botType.WildSpawnTypeValue, new GClass790(botType.IsBoss, botType.IsFollower, botType.IsHostileToEverybody, $"ScavRole/{botType.ScavRole}", (ETagStatus)0));

                if (botType.CountAsBossForStatistics.HasValue)
                {
                    BotSettingsRepoClass.Dictionary_0[(WildSpawnType)botType.WildSpawnTypeValue].CountAsBossForStatistics = botType.CountAsBossForStatistics.Value;
                }
            }

            bool sainLoaded = Chainloader.PluginInfos.ContainsKey("me.sol.sain");

            if (sainLoaded)
            {
                Logger.LogMessage("SAIN detected, initializing SAIN interop for MoreBotsAPI.");
                new SAINInterop().Init();
                //new SAINPatch().Enable();
            }
            else
            {
                Logger.LogMessage("SAIN not detected, skipping SAIN interop for MoreBotsAPI.");
            }

            new TarkovInitPatch().Enable(); //For Sain stuff
            new FixRaidEndSpawnTypePatch().Enable();
            new StandartBotBrainActivatePatch().Enable();
            new SuitableFollowersListPatch().Enable();
            new FenceLoyaltyWarnPatch().Enable();

            int oldWildSpawnTypeConverter = Array.FindIndex<JsonConverter>(JsonSerializerSettingsClass.Converters, c => c.GetType() == typeof(GClass1866<WildSpawnType>));
            LogSource.LogInfo($"Old WildSpawnTypeFromInt converter index: {oldWildSpawnTypeConverter} {JsonSerializerSettingsClass.Converters[oldWildSpawnTypeConverter]}");
            JsonSerializerSettingsClass.Converters[oldWildSpawnTypeConverter] = new WildSpawnTypeFromIntConverter<WildSpawnType>(true);
        }
    }
}
