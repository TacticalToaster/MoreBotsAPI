using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MoreBotsAPI.Components;
using UnityEngine;

namespace MoreBotsAPI.Behavior.Actions
{
    public class HuntRegroupAction : CustomLogic
    {
        private float nextUpdate;
        private BotHuntManager huntManager;
        private FieldInfo botZoneField = null;
        private GClass395 baseSteeringLogic;

        public HuntRegroupAction(BotOwner botOwner) : base(botOwner)
        {
            baseSteeringLogic = new GClass395();

            if (botZoneField == null)
            {
                botZoneField = AccessTools.Field(typeof(BotsGroup), "<BotZone>k__BackingField");
            }
        }

        public override void Start()
        {
            base.Start();

            huntManager = BotOwner.GetComponent<BotHuntManager>();

            BotOwner.Mover.Stop();
            BotOwner.PatrollingData.Pause();
            BotOwner.AimingManager.CurrentAiming.LoseTarget();

            huntManager.regroupPointDirty = true;
        }

        public override void Stop()
        {
            base.Stop();
            updateBotZone();
            BotOwner.PatrollingData.Unpause();
            BotOwner.Sprint(false);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.Sprint(true);
            
            BotOwner.BewarePlantedMine.Update();
            BotOwner.DoorOpener.UpdateDoorInteractionStatus();

            BotOwner.Steering.LookToMovingDirection();
            baseSteeringLogic.Update(BotOwner);

            if (nextUpdate > Time.time) return;

            if (BotOwner.BotsGroup.BossGroup == null || !BotOwner.BotsGroup.BossGroup.Boss.HealthController.IsAlive)
            {
                huntManager.shouldRegroup = false;
            }

            huntManager.UpdateRegroupPoint();

            nextUpdate = Time.time + 3f;

            updateBotZone();

            if (BotOwner.Boss.IamBoss || BotOwner.Position.SqrDistance(huntManager.regroupPoint) < 1f || BotOwner.Position.SqrDistance(BotOwner.BotFollower.BossToFollow.Position) < 10f * 10f)
            {
                huntManager.isRegrouping = false;
            }

            if (!BotOwner.Boss.IamBoss)
            {
                if (BotOwner.Position.SqrDistance(huntManager.regroupPoint) < .75f * .75f)
                { 
                    BotOwner.StopMove();
                }
                else
                    BotOwner.GoToPoint(huntManager.regroupPoint);
            }
            else
            {
                BotOwner.StopMove();

                bool stillRegrouping = false;
                foreach (var manager in huntManager.GetFollowerManagers())
                {
                    if (manager.isRegrouping && !huntManager.botOwner.IsDead)
                    {
                        stillRegrouping = true;
                        break;
                    }
                }
                if (!stillRegrouping)
                {
                    foreach (var manager in huntManager.GetFollowerManagers())
                    {
                        manager.shouldRegroup = false;
                    }

                    huntManager.shouldRegroup = false;
                    return;
                }
            }
        }

        // taken from QuestingBots
        private void updateBotZone()
        {
            BotSpawner botSpawnerClass = Singleton<IBotGame>.Instance.BotsController.BotSpawner;
            BotZone closestBotZone = botSpawnerClass.GetClosestZone(BotOwner.Position, out float dist);

            if (BotOwner.BotsGroup.BotZone == closestBotZone)
            {
                return;
            }

            // Do not allow followers to set the BotZone
            if (!BotOwner.Boss.IamBoss && (BotOwner.BotsGroup.MembersCount > 1))
            {
                return;
            }

            botZoneField.SetValue(BotOwner.BotsGroup, closestBotZone);
            BotOwner.PatrollingData.PointChooser.ShallChangeWay(true);
        }
    }
}
