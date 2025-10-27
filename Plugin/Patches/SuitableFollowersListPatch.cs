using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace MoreBotsAPI.Patches
{
    public class SuitableFollowersListPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSettingsRepoClass), nameof(BotSettingsRepoClass.Init));
        }

        static bool hasRun = false;

        [PatchPostfix]
        public static void PatchPostfix()
        {
            if (hasRun)
                return;

            foreach (var suitableGroup in CustomWildSpawnTypeManager.GetSuitableGroupsList())
            {
                BotSettingsRepoClass.smethod_0(suitableGroup.ConvertAll(type => (WildSpawnType)type));
            }

            hasRun = true;
        }
    }
}
