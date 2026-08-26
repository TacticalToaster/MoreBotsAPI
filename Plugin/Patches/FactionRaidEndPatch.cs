using EFT;
using HarmonyLib;
using JsonType;
using MoreBotsAPI.Components;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;

namespace MoreBotsAPI.Patches;

public class FactionRaidEndPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EftClientBackendSession), nameof(EftClientBackendSession.LocalRaidEnded));
    }

    [PatchPostfix]
    public static void PatchPostfix(LocalRaidSettings settings, SessionResult results, FlatItem[] lostInsuredItems, Dictionary<string, FlatItem[]> transferItems)
    {
        var factionManager = MonoBehaviourSingleton<FactionManager>.Instance;
        
        factionManager.SendRevenges();
    }
}