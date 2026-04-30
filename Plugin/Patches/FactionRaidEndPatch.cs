using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using MoreBotsAPI.Components;
using SPT.Reflection.Patching;

namespace MoreBotsAPI.Patches;

public class FactionRaidEndPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Class308), nameof(Class308.LocalRaidEnded));
    }

    [PatchPostfix]
    public static void PatchPostfix(LocalRaidSettings settings, RaidEndDescriptorClass results, FlatItemsDataClass[] lostInsuredItems, Dictionary<string, FlatItemsDataClass[]> transferItems)
    {
        var factionManager = MonoBehaviourSingleton<FactionManager>.Instance;
        
        factionManager.SendRevenges();
    }
}