using System.Collections.Generic;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using MoreBotsAPI.Interop;
using MoreBotsAPI.Models;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.SinglePlayer.Utils.InRaid;

namespace MoreBotsAPI.Components;

public class FactionManager : MonoBehaviourSingleton<FactionManager>
{
    public Dictionary<string, Faction> Factions = new();
    public Dictionary<string, List<string>> RevengeRaidsLeftByProfile = new();
    public Dictionary<string, List<string>> UpdateRevengeFactions = new();
    public Dictionary<WildSpawnType, List<string>> WildSpawnTypeFactionDict = new();

    [CanBeNull]
    public List<string> GetFactionsByRole(WildSpawnType role)
    {
        if (WildSpawnTypeFactionDict.TryGetValue(role, out var factions))
        {
            return factions;
        }
        else
        {
            return null;
        }
    }

    public bool IsRoleInFaction(WildSpawnType role, string factionName)
    {
        return WildSpawnTypeFactionDict.TryGetValue(role, out var factions) && factions.Contains(factionName);
    }

    public bool ShouldRevengeByID(string profileID, string faction)
    {
        if (!RevengeRaidsLeftByProfile.TryGetValue(profileID, out var revengeFactions)) return false;
        
        foreach (var revengeFaction in revengeFactions)
        {
            if (faction == revengeFaction) return true;
        }
        return false;
    }

    public bool ShouldBeRevenged(BotsGroup botsGroup, IPlayer player)
    {
        var profileID = player.ProfileId;
        
        if (!RevengeRaidsLeftByProfile.TryGetValue(profileID, out var revengeFactions)) return false;

        foreach (var revengeFaction in revengeFactions)
        {
            if (IsRoleInFaction(botsGroup.InitialBot.Profile.Info.Settings.Role, revengeFaction)) return true;
        }
        return false;
    }

    public void UpdateRevengeByRole(WildSpawnType role, string playerID)
    {
        if (!WildSpawnTypeFactionDict.TryGetValue(role, out var factions)) return;

        foreach (var factionName in factions)
        {
            var faction = Factions[factionName];
            
            if (!faction.RevengeAfterRaids) continue;

            if (!UpdateRevengeFactions.TryGetValue(playerID, out var revengeFactions))
            {
                UpdateRevengeFactions[playerID] = new List<string>();
            }
            
            if (UpdateRevengeFactions[playerID].Contains(factionName)) continue;
            
            UpdateRevengeFactions[playerID].Add(factionName);
        }
    }

    public void LoadFactions()
    {
        var result = RequestHandler.GetJson("/morebotsapi/getfactions");
        Plugin.LogSource.LogInfo($"[MOREBOTSAPI] Loading factions from server...");
        var factions = JsonConvert.DeserializeObject<Dictionary<string, Faction>>(result);
        
        Factions = factions;
        WildSpawnTypeFactionDict = new Dictionary<WildSpawnType, List<string>>();

        foreach ((string factionName, Faction faction) in factions)
        {
            foreach (var factionRole in faction.GetAllBotTypes())
            {
                if (WildSpawnTypeFactionDict.TryGetValue(factionRole, out var roleFactions))
                {
                    if (!roleFactions.Contains(factionName))
                        roleFactions.Add(factionName);
                }
                else
                {
                    WildSpawnTypeFactionDict[factionRole] = new List<string>() { factionName };
                }
            }
        }
    }

    public void LoadRevenges()
    {
        var result = RequestHandler.GetJson("/morebotsapi/getrevenges");
        Plugin.LogSource.LogInfo($"[MOREBOTSAPI] Loading faction raid revenges from server...");
        var revenges = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(result);
        
        RevengeRaidsLeftByProfile = revenges;
    }

    public void SendRevenges()
    {
        if (Plugin.FikaInitialized && !FikaInterop.IsServer()) return;

        var request = new UpdateRevengeRequest();
        request.RevengeUpdate = UpdateRevengeFactions;
        
        var revengeSerialized = Json.Serialize(request);
        
        RequestHandler.PutJson(
            $"/morebotsapi/updaterevenge", revengeSerialized);
        
        UpdateRevengeFactions.Clear();
    }

    public void InitRaid(BotsController botsController)
    {
        LoadFactions();
        LoadRevenges();
        
        if (!Singleton<BotEventHandler>.Instantiated) return;
        
        Singleton<BotEventHandler>.Instance.OnKill -= OnKill;
        Singleton<BotEventHandler>.Instance.OnKill += OnKill;
    }

    public void OnKill(IPlayer killer, IPlayer victim)
    {
        if (killer.IsAI || !victim.IsAI) return;

        var role = victim.Profile.Info.Settings.Role;

        UpdateRevengeByRole(role, killer.ProfileId);
    }

    public override void OnDestroy()
    {
        if (!Singleton<BotEventHandler>.Instantiated) return;
        
        Singleton<BotEventHandler>.Instance.OnKill -= OnKill;
    }
}