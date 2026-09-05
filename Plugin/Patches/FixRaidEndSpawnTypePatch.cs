using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace MoreBotsAPI.Patches
{
    public class FixRaidEndSpawnTypePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BaseStatisticsManager).GetMethod(nameof(BaseStatisticsManager.OnDeath), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(BaseStatisticsManager __instance)
        {
            var role = __instance.Profile.EftStats.DeathCause.Role;
            if (CustomWildSpawnTypeManager.IsCustomWildSpawnType((int)role))
                __instance.Profile.EftStats.DeathCause.Role = WildSpawnType.assault;
        }
    }
}
