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
    internal class HuntTargetAction : CustomLogic
    {
        private float nextUpdate;
        private BotHuntManager huntManager;
        private FieldInfo botZoneField = null;
        private GClass395 baseSteeringLogic;

        public HuntTargetAction(BotOwner botOwner) : base(botOwner)
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
        }

        public override void Stop()
        {
            base.Stop();
            updateBotZone();
            BotOwner.PatrollingData.Unpause();
        }

        public override void Update(CustomLayer.ActionData data)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.Sprint(false);

            BotOwner.BewarePlantedMine.Update();
            BotOwner.DoorOpener.UpdateDoorInteractionStatus();
            
            
            BotOwner.Steering.LookToMovingDirection();
            baseSteeringLogic.Update(BotOwner);

            if (nextUpdate > Time.time) return;

            nextUpdate = Time.time + 3f;

            updateBotZone();

            BotOwner.GoToPoint(huntManager.knownLocation, mustHaveWay: false);

            if (BotOwner.Boss.IamBoss)
            {
                huntManager.shouldRegroup = huntManager.CheckShouldRegroup();
            }

            if (BotOwner.Position.SqrDistance(huntManager.knownLocation) < 5f * 5f)
            {
                huntManager.shouldSearch = true;
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
