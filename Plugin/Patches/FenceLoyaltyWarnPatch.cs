using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MoreBotsAPI.Patches
{
    public class FenceLoyaltyWarnPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGroupWarnData), nameof(BotGroupWarnData.method_9));
        }

        [PatchPostfix]
        public static void PatchPostfix(ref bool __result, BotGroupWarnData __instance, Player enemyInfo)
        {
            var role = __instance.Boss.Profile.Info.Settings.Role;

            if (role.IsCustomType())
            {
                var customType = role.GetCustomType();

                if (enemyInfo.Side == EPlayerSide.Savage && customType.ShouldUseFenceNoBossAttackScav)
                {
                    __result = enemyInfo.Loyalty.BossNoAttack;
                    return;
                }

                if (enemyInfo.Side != EPlayerSide.Savage && customType.ShouldUseFenceNoBossAttackPMC)
                {
                    __result = enemyInfo.Loyalty.BossNoAttack;
                    return;
                }

                __result = false;
            }
        }
    }
}
