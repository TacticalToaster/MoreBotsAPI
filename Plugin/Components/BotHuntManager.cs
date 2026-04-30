using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace MoreBotsAPI.Components
{
    public class BotHuntManager : MonoBehaviour
    {
        public BotOwner botOwner;
        public HuntManager huntManager;
        public IPlayer huntTarget;
        public Vector3 knownLocation;
        public Vector3 regroupPoint;
        public bool regroupPointDirty = false;
        public bool isRegrouping = false;
        public bool shouldRegroup = false;
        public bool ignoreRegroup = false;
        public bool shouldSearch = false;
        public bool active = false;
        public List<BotHuntManager> followerManagers;
        public List<IPlayer> priorityTargets = new();

        private float updateTime;
        private float locationTime;
        private float waitTime;

        public void Update()
        {
            if (!active) return;

            if (Time.time > updateTime)
            {
                if (huntManager.huntGroups.TryGetValue(botOwner.BotsGroup, out var target))
                    huntTarget = target;

                updateTime = Time.time + 3f;

                if (!huntTarget?.HealthController?.IsAlive ?? true)
                {
                    huntManager.FindNewHuntTarget(this);
                }
            }

            if (Time.time > locationTime && HasHuntTarget())
            {
                locationTime = Time.time + 30f;

                knownLocation = huntTarget.Position;
            }
            
        }

        public void Init(BotOwner bot, HuntManager manager)
        {
            botOwner = bot;
            huntManager = manager;
            active = true;
            waitTime = Time.time + 10f;
        }

        public bool HasHuntTarget()
        {
            return active && Time.time > waitTime && huntTarget != null && huntTarget.HealthController.IsAlive;
        }

        public Vector3 GetRegroupPoint()
        {
            if (botOwner.Boss.IamBoss) return Vector3.zero;

            var regroupPoint = botOwner.BotFollower.BossToFollow.Position;
            var randomDisc = UnityEngine.Random.insideUnitCircle * 5f;
            regroupPoint = regroupPoint + new Vector3(randomDisc.x, 0, randomDisc.y);

            if (NavMesh.SamplePosition(regroupPoint, out var hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return botOwner.BotFollower.BossToFollow.Position;

            /*var searchData = new CoverSearchData(regroupPoint, botOwner.CoverSearchInfo, CoverShootType.shoot, 10f * 10f, 0f, CoverSearchType.distToBot, null, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(botOwner.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL), PointsArrayType.byShootType, true);
            var coverPoint = botOwner.BotsGroup.CoverPointMaster.GetCoverPointMain(searchData, true);
            botOwner.Memory.SetCoverPoints(coverPoint);
            return coverPoint;*/
        }

        public void UpdateRegroupPoint()
        {
            if (regroupPointDirty)
            {
                regroupPoint = GetRegroupPoint();
                regroupPointDirty = false;
            }
        }

        public List<BotHuntManager> GetFollowerManagers()
        {
            if (followerManagers == null)
                followerManagers = new List<BotHuntManager>();

            if (!botOwner.Boss.IamBoss) return followerManagers;

            if (botOwner.BotsGroup.MembersCount > followerManagers.Count)
            {
                followerManagers.Clear();

                foreach (var follower in botOwner.BotsGroup.Members)
                {
                    if (follower.TryGetComponent<BotHuntManager>(out var manager))
                    {
                        followerManagers.Add(manager);
                    }
                }
            }

            return followerManagers; 
        }

        public bool CheckShouldRegroup()
        {
            if (!botOwner.Boss.IamBoss) return false;

            var managers = GetFollowerManagers();
            var regroup = false;

            foreach (var manager in managers)
            {
                if (manager != this && manager.botOwner.Position.SqrDistance(botOwner.Position) > 20f * 20f)
                {
                    regroup = true;
                    break;
                }
            }

            if (!regroup) return false;

            foreach (var manager in managers)
            {
                manager.shouldRegroup = true;

                manager.isRegrouping = true;
            }

            shouldRegroup = true;
            isRegrouping = true;

            return true;
        }
    }
}
