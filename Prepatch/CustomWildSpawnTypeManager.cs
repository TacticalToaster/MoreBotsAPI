using Mono.Cecil;
using MoreBotsAPI.Prepatch;
using System.Collections.Generic;

namespace MoreBotsAPI
{
    public static class CustomWildSpawnTypeManager
    {
        private static List<CustomWildSpawnType> _customWildSpawnTypes = new List<CustomWildSpawnType>();
        private static Dictionary<int, CustomWildSpawnType> _customWildSpawnTypeDict = new Dictionary<int, CustomWildSpawnType>();
        private static List<List<int>> _suitableGroupsList = new List<List<int>>();
        private static List<SAINSettings> _sainSettings = new List<SAINSettings>();

        private static int _spawnTypeIndex = 1000;

        private static void AddType(CustomWildSpawnType customType)
        {
            _customWildSpawnTypes.Add(customType);
            _customWildSpawnTypeDict[customType.WildSpawnTypeValue] = customType;

            if (customType.SAINSettings != null)
            {
                _sainSettings.Add(customType.SAINSettings);
            }
        }

        public static void RegisterWildSpawnType(AssemblyDefinition assembly, string typeName, string scavRole, int baseBrain, bool isBoss = false, bool isFollower = false, bool isHostileToEverybody = false, bool countAsBossForStatistics = false)
        {
            CustomWildSpawnType newType = new CustomWildSpawnType(_spawnTypeIndex, typeName, scavRole, baseBrain, isBoss, isFollower, isHostileToEverybody);
            _spawnTypeIndex++;

            newType.SetCountAsBossForStatistics(countAsBossForStatistics);

            RegisterWildSpawnType(newType, assembly);
        }

        // Register your custom bot types with this
        public static void RegisterWildSpawnType(CustomWildSpawnType customType, AssemblyDefinition assembly)
        {
            var wildSpawnType = assembly.MainModule.GetType("EFT.WildSpawnType");

            Utils.AddEnumValue(ref wildSpawnType, customType.WildSpawnTypeName, customType.WildSpawnTypeValue);

            AddType(customType);
        }

        // Create a list of ints that correspond to the WildSpawnType value that can form a group (e.g., a boss and its followers, or a squad of the same type)
        // For example, to create a group with a boss of WildSpawnType 1000 and two followers of WildSpawnType 1001 and 1002 make a list [1000, 1001, 1002] and pass it to this function
        // Pass a list of just a single value to allow that type to form groups with itself (like raiders and rogues)
        public static void AddSuitableGroup(List<int> suitableGroup)
        {
            _suitableGroupsList.Add(suitableGroup);
        }

        public static List<CustomWildSpawnType> GetCustomWildSpawnTypes()
        {
            return _customWildSpawnTypes;
        }

        public static Dictionary<int, CustomWildSpawnType> GetCustomWildSpawnTypeDict()
        {
            return _customWildSpawnTypeDict;
        }

        public static bool IsCustomWildSpawnType(int type)
        {
            return _customWildSpawnTypeDict.ContainsKey(type);
        }

        public static List<List<int>> GetSuitableGroupsList()
        {
            return _suitableGroupsList;
        }

        public static List<SAINSettings> GetSAINSettings()
        {
            return _sainSettings;
        }
    }
}
