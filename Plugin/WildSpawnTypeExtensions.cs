﻿using EFT;

namespace MoreBotsAPI
{
    public static class WildSpawnTypeExtensions
    {
        public static bool IsCustomType(this WildSpawnType wildSpawnType)
        {
            return CustomWildSpawnTypeManager.GetCustomWildSpawnTypeDict()[(int)wildSpawnType] != null;
        }

        public static CustomWildSpawnType GetCustomType(this WildSpawnType wildSpawnType)
        {
            return CustomWildSpawnTypeManager.GetCustomWildSpawnTypeDict()[(int)wildSpawnType];
        }
    }
}
