using Comfort.Common;
using EFT;
using SPT.SinglePlayer.Utils.InRaid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreBotsAPI;
using UnityEngine;

namespace MoreBotsAPI.Components
{
    public class HuntManager : MonoBehaviourSingleton<HuntManager>
    {

        public Dictionary<BotsGroup, IPlayer> huntGroups = new();
        public Dictionary<string, WildSpawnType> huntEvents = new() {
            { "blackDivHunt", (WildSpawnType)848421 }
        };

        private bool startNewHunt = false;
        private WildSpawnType huntRole;
        private BotsGroup newHuntGroup;
        private List<string> unpickedHunts = new List<string>();

        private float nextUpdate = 0f;

        public void Update()
        {

            if (nextUpdate > Time.time) return;
            
            if (Singleton<GameWorld>.Instantiated == false ||
                Singleton<GameWorld>.Instance is HideoutGameWorld)
            {
                nextUpdate = Time.time + 10f;
                return;
            }

            nextUpdate = Time.time + 240f;

            if (UnityEngine.Random.Range(0, 100) < 8 && unpickedHunts.Count > 0)
            {
                var randomEvent = unpickedHunts.Random();
                if (randomEvent == null) return;
                unpickedHunts.Remove(randomEvent);
                StartHunt(randomEvent);
            }
        }

        public void StartHunt(string huntEvent)
        {
            huntRole = huntEvents[huntEvent];
            startNewHunt = true;
            Singleton<BotEventHandler>.Instance.AnyEvent(huntEvent);
            Plugin.LogSource.LogInfo($"[MoreBotsAPI] Starting hunt event {huntEvent}");
        }

        public void InitRaid()
        {
            unpickedHunts = [.. huntEvents.Keys];
            
            Singleton<BotEventHandler>.Instance.AnyEvent("hunt");

            nextUpdate = Time.time + 240f;

            Singleton<IBotGame>.Instance.BotsController.BotSpawner.OnBotCreated += OnBotCreated;
        }

        public void OnBotCreated(BotOwner bot)
        {
            if (!bot.SpawnProfileData.SpawnParams.Id_spawn.ToLower().Contains("hunt")) return;

            var huntManager = bot.gameObject.GetOrAddComponent<BotHuntManager>();

            if (huntManager.huntTarget != null) return;
            
            huntManager.Init(bot, this);
            FindFirstHuntTarget(huntManager);
        }

        public void AddHuntTarget(BotsGroup hunters, IPlayer hunted)
        {
            huntGroups.Add(hunters, hunted);
        }

        public void FindFirstHuntTarget(BotHuntManager hunter)
        {
            var role = hunter.botOwner.Profile.Info.Settings.Role;
            var allBots = Singleton<GameWorld>.Instance.AllAlivePlayersList.Randomize();

            if (huntGroups.TryGetValue(hunter.botOwner.BotsGroup, out var player))
            {
                hunter.huntTarget = player;
                return;
            }

            foreach (var bot in allBots)
            {
                if (!bot.HealthController.IsAlive) continue;

                var targetRole = bot.Profile.Info.Settings.Role;
                var targetSide = bot.Profile.Side;

                if (
                    (bot.IsAI && validHuntRoles.TryGetValue(role, out var huntList) && huntList.Contains(targetRole)) ||
                    (validPMCHunts.TryGetValue(role, out var sideList) &&  sideList.Contains(targetSide))
                    )
                {
                    if (!huntGroups.ContainsKey(hunter.botOwner.BotsGroup))
                    {
                        AddHuntTarget(hunter.botOwner.BotsGroup, bot);
                    }

                    hunter.huntTarget = huntGroups[hunter.botOwner.BotsGroup];
                    return;
                }
            }
        }

        public void FindNewHuntTarget(BotHuntManager hunter)
        {
            var role = hunter.botOwner.Profile.Info.Settings.Role;
            var allBots = Singleton<GameWorld>.Instance.AllAlivePlayersList.Randomize();

            foreach (var bot in allBots)
            {
                if (!bot.HealthController.IsAlive) continue;

                var targetRole = bot.Profile.Info.Settings.Role;
                var targetSide = bot.Profile.Side;

                if (
                    (bot.IsAI && validHuntRoles.TryGetValue(role, out var huntList) && huntList.Contains(targetRole)) ||
                    (validPMCHunts.TryGetValue(role, out var sideList) &&  sideList.Contains(targetSide))
                )
                {
                    if (!huntGroups.ContainsKey(hunter.botOwner.BotsGroup))
                    {
                        AddHuntTarget(hunter.botOwner.BotsGroup, bot);
                    }
                    else
                    {
                        if (bot == huntGroups[hunter.botOwner.BotsGroup]) continue;

                        huntGroups[hunter.botOwner.BotsGroup] = bot;
                    }

                    foreach (var follower in hunter.botOwner.BotsGroup.Members)
                    {
                        follower.GetComponent<BotHuntManager>().huntTarget = huntGroups[hunter.botOwner.BotsGroup];
                    }

                    
                    return;
                }
            }
        }

        public void AddHuntRoles(WildSpawnType hunter, List<WildSpawnType> hunted)
        {
            if (validHuntRoles.ContainsKey(hunter))
            {
                validHuntRoles[hunter].AddRange(hunted);
                validHuntRoles[hunter] = validHuntRoles[hunter].Distinct().ToList();
            }
            else
                validHuntRoles.Add(hunter, hunted);
        }

        public void AddHuntRoles(List<WildSpawnType> hunters, List<WildSpawnType> hunted)
        {
            foreach (var hunter in hunters)
            {
                AddHuntRoles(hunter, hunted);
            }
        }
        
        public void AddHuntSides(WildSpawnType hunter, List<EPlayerSide> hunted)
        {
            if (validPMCHunts.ContainsKey(hunter))
            {
                validPMCHunts[hunter].AddRange(hunted);
                validPMCHunts[hunter] = validPMCHunts[hunter].Distinct().ToList();
            }
            else
                validPMCHunts.Add(hunter, hunted);
        }

        public void AddHuntSides(List<WildSpawnType> hunters, List<EPlayerSide> hunted)
        {
            foreach (var hunter in hunters)
            {
                AddHuntSides(hunter, hunted);
            }
        }

        public readonly Dictionary<WildSpawnType, List<EPlayerSide>> validPMCHunts = new() {};

        public readonly Dictionary<WildSpawnType, List<WildSpawnType>> validHuntRoles = new() {};
    }
}
