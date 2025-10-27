using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreBotsAPI.Prepatch;

namespace MoreBotsAPI
{
    public static class CustomWildSpawnTypeManager
    {
        private static List<CustomWildSpawnType> customWildSpawnTypes = new List<CustomWildSpawnType>();
        private static Dictionary<int, CustomWildSpawnType> customWildSpawnTypeDict = new Dictionary<int, CustomWildSpawnType>();
        private static List<List<int>> suitableGroupsList = new List<List<int>>();

        private static int spawnTypeIndex = 1000;

        public static void AddType(CustomWildSpawnType customType)
        {
            customWildSpawnTypes.Add(customType);
            customWildSpawnTypeDict[customType.WildSpawnTypeValue] = customType;
        }

        public static void AddType(string typeName, string scavRole, int baseBrain, bool isBoss = false, bool isFollower = false, bool isHostileToEverybody = false, bool countAsBossForStatistics = false)
        {
            CustomWildSpawnType newType = new CustomWildSpawnType(spawnTypeIndex, typeName, scavRole, baseBrain, isBoss, isFollower, isHostileToEverybody);
            spawnTypeIndex++;

            newType.SetCountAsBossForStatistics(countAsBossForStatistics);

            AddType(newType);
        }

        // Create a list of ints that correspond to the WildSpawnType value that can form a group (e.g., a boss and its followers, or a squad of the same type)
        // For example, to create a group with a boss of WildSpawnType 1000 and two followers of WildSpawnType 1001 and 1002 make a list [1000, 1001, 1002] and pass it to this function
        // Pass a list of just a single value to allow that type to form groups with itself (like raiders and rogues)
        public static void AddSuitableGroup(List<int> suitableGroup)
        {
            suitableGroupsList.Add(suitableGroup);
        }

        public static List<CustomWildSpawnType> GetCustomWildSpawnTypes()
        {
            return customWildSpawnTypes;
        }
    }
}
