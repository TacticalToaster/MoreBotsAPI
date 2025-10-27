using DrakiaXYZ.BigBrain.Brains;
using EFT;
using HarmonyLib;
using SAIN.Attributes;
using SAIN.Preset;
using SAIN.Preset.BotSettings;
using SAIN.Preset.BotSettings.SAINSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

                BrainManager.RemoveLayers(layers, setting.BrainsToApply, new List<WildSpawnType> { (WildSpawnType)setting.WildSpawnType });
            }
        }

        // I literally just ripped stuff from SAIN to replicate how it does it since the init for settings happens immediately and there's no chance to register custom bot types

        public static FieldInfo[] GetFieldsInType(Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            return type.GetFields(flags);
        }

        private static void CopyValuesAtoB(object A, object B, Func<FieldInfo, bool> shouldCopyFieldFunc = null)
        {
            // Get the names of the fields in EFT group
            List<string> ACatNames = AccessTools.GetFieldNames(A);
            foreach (FieldInfo BCatField in GetFieldsInType(B.GetType()))
            {
                // Check if the category inside SAIN GlobalSettings has a matching category in EFT group
                if (ACatNames.Contains(BCatField.Name))
                {
                    // Get the multiplier of the category from SAIN group
                    object BCatObject = BCatField.GetValue(B);
                    // Get the fields inside that category from SAIN group
                    FieldInfo[] BVariableFieldArray = GetFieldsInType(BCatField.FieldType);

                    // Get the category of the matching sain category from EFT group
                    FieldInfo ACatField = AccessTools.Field(A.GetType(), BCatField.Name);
                    if (ACatField != null)
                    {
                        // Get the value of the EFT group Category
                        object ACatObject = ACatField.GetValue(A);
                        // list the field names in that category
                        List<string> AVariableNames = AccessTools.GetFieldNames(ACatObject);

                        foreach (FieldInfo BVariableField in BVariableFieldArray)
                        {
                            // Check if the sain variable is set to grab default EFT numbers and that it exists inside the EFT group category
                            if (AVariableNames.Contains(BVariableField.Name))
                            {
                                if (shouldCopyFieldFunc != null && !shouldCopyFieldFunc(BVariableField))
                                {
                                    continue;
                                }
                                // Get the Variable from this category that matched
                                FieldInfo AVariableField = AccessTools.Field(ACatObject.GetType(), BVariableField.Name);
                                if (AVariableField != null)
                                {
                                    // Get the final Rounding of the variable from EFT group, and set the SAIN Setting variable to that multiplier
                                    object AValue = AVariableField.GetValue(ACatObject);
                                    BVariableField.SetValue(BCatObject, AValue);
                                    //Logger.LogWarning($"Set [{BVariableField.LayerName}] to [{AValue}]");
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool ShallUseEFTBotDefault(FieldInfo field) => AttributesGUI.GetAttributeInfo(field)?.CopyValue == true;

        private static void UpdateSAINSettingsToEFTDefault(WildSpawnType wildSpawnType, SAINSettingsGroupClass sainSettingsGroup)
        {
            var Preset = SAINPresetClass.Instance;
            var botSettings = Preset.BotSettings;

            foreach (var keyPair in sainSettingsGroup.Settings)
            {
                SAINSettingsClass sainSettings = keyPair.Value;
                BotDifficulty Difficulty = keyPair.Key;

                // Get SAIN and EFT group for the given WildSpawnType and difficulties
                object eftSettings = botSettings.GetEFTSettings(wildSpawnType, Difficulty);
                if (eftSettings != null)
                {
                    CopyValuesAtoB(eftSettings, sainSettings, (field) => ShallUseEFTBotDefault(field));
                }
            }
        }

        public static void CreateCustomBotTypes()
        {
            Plugin.LogSource.LogInfo("Creating custom bot types for SAIN...");

            var Preset = SAINPresetClass.Instance;
            var botSettings = Preset.BotSettings;
            BotDifficulty[] Difficulties = { BotDifficulty.easy, BotDifficulty.normal, BotDifficulty.hard, BotDifficulty.impossible };

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
                BotTypeDefinitions.BotTypesList.Add(botType);
                BotTypeDefinitions.BotTypes.Add(botType.WildSpawnType, botType);
                BotTypeDefinitions.BotTypesNames.Add(botType.Name);

                SAINBotSettingsClass.DefaultDifficultyModifier.Add(botType.WildSpawnType, 0.5f);

                var wildSpawnType = botType.WildSpawnType;
                var name = botType.Name;

                SAINSettingsGroupClass sainSettingsGroup;
                if (Preset.Info.IsCustom == false || !SAINPresetClass.Import(out sainSettingsGroup, Preset.Info.Name, name, "BotSettings"))
                {
                    sainSettingsGroup = new SAINSettingsGroupClass(Difficulties)
                    {
                        Name = name,
                        WildSpawnType = wildSpawnType,
                        DifficultyModifier = SAINBotSettingsClass.DefaultDifficultyModifier[wildSpawnType]
                    };

                    UpdateSAINSettingsToEFTDefault(wildSpawnType, sainSettingsGroup);

                    if (Preset.Info.IsCustom == true)
                    {
                        SAINPresetClass.Export(sainSettingsGroup, Preset.Info.Name, name, "BotSettings");
                    }
                }

                botSettings.SAINSettings.Add(wildSpawnType, sainSettingsGroup);

                Plugin.LogSource.LogInfo($"Added SAIN BotType: {botType.Name} with WildSpawnType {botType.WildSpawnType}");
            }
        }
    }
}
