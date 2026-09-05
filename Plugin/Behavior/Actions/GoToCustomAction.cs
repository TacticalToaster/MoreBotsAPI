using DrakiaXYZ.BigBrain.Brains;
using EFT;
using EFT.UI.Builds;
using MoreBotsAPI.Components;
using System.Text;

namespace MoreBotsAPI.Behavior.Actions
{
    public abstract class GoToCustomAction : CustomLogic
    {
        private LookAround baseSteeringLogic;
        private GoToCoverPoint goToCoverPoint;
        private MoveToCoverActionResultData goToData;

        public GoToCustomAction(BotOwner botOwner) : base(botOwner)
        {
            goToCoverPoint = new GoToCoverPoint(BotOwner);
            baseSteeringLogic = new LookAround();
        }

        public override void Start()
        {
            SetDataPoint(GetGoToPoint());

            BotOwner.AimingManager.CurrentAiming.LoseTarget();

            base.Start();
        }

        public override void Stop()
        {
            BotOwner.Mover.Sprint(false);
            base.Stop();
        }

        public override void Update(CustomLayer.ActionData data)
        {
            SetDataPoint(GetGoToPoint());
            goToCoverPoint.UpdateNodeByMain(goToData);

            UpdateBotMovement();
            UpdateSteering();
        }

        public void UpdateBotMovement()
        {
            BotOwner.SetPose(1f);
            BotOwner.BotLay.GetUp(true);

            BotOwner.Mover.Sprint(false);
            BotOwner.SetTargetMoveSpeed(1f);
        }

        public void UpdateSteering()
        {
            BotOwner.Steering.LookToMovingDirection();
            baseSteeringLogic.Update(BotOwner);
        }

        private void SetDataPoint(CustomNavigationPoint point)
        {
            if (goToData == null)
                goToData = new(point);

            goToData.Point = point;
        }

        public abstract CustomNavigationPoint GetGoToPoint();
    }
}
