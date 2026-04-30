using DrakiaXYZ.BigBrain.Brains;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreBotsAPI.Components;
using UnityEngine;

namespace MoreBotsAPI.Behavior.Actions
{

    public class SearchForTargetAction : CustomLogic
    {
        private BotNodeAbstractClass baseAction;
        private float endTime;
        private BotHuntManager huntManager;

        public SearchForTargetAction(BotOwner botOwner) : base(botOwner)
        {
            if (botOwner.Boss.IamBoss)
                baseAction = BotActionNodesClass.CreateNode(BotLogicDecision.simplePatrol, botOwner);
            else
                baseAction = BotActionNodesClass.CreateNode(BotLogicDecision.followerPatrol, botOwner);
        }

        public override void Start()
        {
            base.Start();

            huntManager = BotOwner.GetComponent<BotHuntManager>();

            BotOwner.Mover.Stop();

            endTime = Time.time + UnityEngine.Random.Range(12f, 15f);
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void Update(CustomLayer.ActionData data)
        {
            baseAction.UpdateNodeByMain(data);

            if (endTime < Time.time)
            {
                huntManager.shouldSearch = false;
            }
        }
    }
}
