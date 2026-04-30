using DrakiaXYZ.BigBrain.Brains;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreBotsAPI.Behavior.Actions;
using MoreBotsAPI.Components;

namespace MoreBotsAPI.Behavior.Layers
{
    public class HuntTargetLayer : CustomLayer
    {
        public BotHuntManager huntManager;

        public Type lastAction;
        public Type nextAction;
        public string nextActionReason;

        public override string GetName()
        {
            return "HuntTarget";
        }

        public HuntTargetLayer(BotOwner botOwner, int priority) : base(botOwner, priority)
        {
            huntManager = BotOwner.GetOrAddComponent<BotHuntManager>();
        }

        public override void Start()
        {
            base.Start();
            huntManager = BotOwner.GetOrAddComponent<BotHuntManager>();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public void setNextAction(Type actionType, string reason)
        {
            nextAction = actionType;
            nextActionReason = reason;
        }

        public override Action GetNextAction()
        {
            lastAction = nextAction;

            return new Action(lastAction, nextActionReason);
        }

        public override bool IsActive()
        {
            if (huntManager == null || !huntManager.HasHuntTarget())
                return false;
            
            Plugin.LogSource.LogMessage("HUNT IS ACTIVE");


            getNextAction();
            
            Plugin.LogSource.LogMessage("HUNT REALLY IS ACTIVE");
            return true;
        }

        public void getNextAction()
        {
            lastAction = nextAction;
            if (huntManager.shouldRegroup && !huntManager.ignoreRegroup)
            {
                nextAction = typeof(HuntRegroupAction);
                nextActionReason = "ShouldRegroup";
                return;
            }
            if (huntManager.shouldSearch)
            {
                nextAction = typeof(SearchForTargetAction);
                nextActionReason = "SearchingForTarget";
                return;
            }
            nextAction = typeof(HuntTargetAction);
            nextActionReason = "HuntingTarget";
        }

        public override bool IsCurrentActionEnding()
        {
            return nextAction != lastAction || (CurrentAction.Type != nextAction && CurrentAction.Type != lastAction);
        }
    }
}
