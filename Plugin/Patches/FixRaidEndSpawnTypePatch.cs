using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace MoreBotsAPI.Patches
{
    public class FixRaidEndSpawnTypePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocationStatisticsCollectorAbstractClass).GetMethod(nameof(LocationStatisticsCollectorAbstractClass.OnDeath), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(LocationStatisticsCollectorAbstractClass __instance)
        {
            var role = __instance.Profile_0.EftStats.DeathCause.Role;
            if (role.IsCustomType())
                __instance.Profile_0.EftStats.DeathCause.Role = WildSpawnType.assault;
        }
    }
}
