using System.Reflection;
using EFT;
using HarmonyLib;
using MoreBotsAPI.Components;
using SPT.Reflection.Patching;

namespace MoreBotsAPI.Patches;

public class BotsGroupIsPlayerEnemyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.IsPlayerEnemy));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref bool __result, BotsGroup __instance, IPlayer player)
    {
        if (__result) return;

        if (player.IsAI) return;

        var factionManager = MonoBehaviourSingleton<FactionManager>.Instance;
        
        if (factionManager.ShouldBeRevenged(__instance, player)) __result = true;
    }
}