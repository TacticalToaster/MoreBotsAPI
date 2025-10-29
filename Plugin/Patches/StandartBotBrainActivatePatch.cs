using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace MoreBotsAPI.Patches
{
    public class StandartBotBrainActivatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(StandartBotBrain), nameof(StandartBotBrain.Activate));
        }

        [PatchPostfix]
        [HarmonyPriority(Priority.First)] // Make sure this runs after BigBrain so we can override it
        public static void PatchPostfix(StandartBotBrain __instance, BotOwner ___BotOwner_0)
        {
            if (___BotOwner_0.Profile.Info.Settings.Role.IsCustomType())
            {
                var customType = ___BotOwner_0.Profile.Info.Settings.Role.GetCustomType();
                Logger.LogMessage("Changing brain for custom bot.");
                __instance.BaseBrain = GetBaseBrain(___BotOwner_0, customType.BaseBrain);
                __instance.Agent = GetAgent(___BotOwner_0, __instance.BaseBrain, __instance);

                // This isn't critical, only knight, sanitar, and the attack event (khorvod generators, spring event, etc) behaviors use this
                //__instance.OnSetStrategy?.Invoke(__instance.BaseBrain);
            }
        }

        public static AICoreAgentClass<BotLogicDecision> GetAgent(BotOwner botOwner, BaseBrain baseBrain, StandartBotBrain standartBotBrain)
        {
            var name = botOwner.name + " " + botOwner.Profile.Info.Settings.Role.ToString();
            return new AICoreAgentClass<BotLogicDecision>(botOwner.BotsController.AICoreController, baseBrain, BotActionNodesClass.ActionsList(botOwner), botOwner.gameObject, name, new Func<BotLogicDecision, BotNodeAbstractClass>(standartBotBrain.method_0));
        }

        public static BaseBrain GetBaseBrain(BotOwner botOwner, int brainType)
        {
            BaseBrain baseBrain;

            switch ((WildSpawnType)brainType)
            {
                case WildSpawnType.marksman:
                    baseBrain = new GClass346(botOwner);
                    break;
                case WildSpawnType.bossTest:
                    baseBrain = new GClass320(botOwner);
                    break;
                case WildSpawnType.bossBully:
                    baseBrain = new GClass314(botOwner);
                    break;
                case WildSpawnType.followerBully:
                    baseBrain = new GClass332(botOwner);
                    break;
                case WildSpawnType.bossKilla:
                    baseBrain = new GClass342(botOwner);
                    break;
                case WildSpawnType.bossKojaniy:
                    baseBrain = new GClass357(botOwner);
                    break;
                case WildSpawnType.followerKojaniy:
                    baseBrain = new BossKojaniyBrainClass(botOwner);
                    break;
                case WildSpawnType.pmcBot:
                case WildSpawnType.arenaFighterEvent:
                    baseBrain = new GClass349(botOwner, false);
                    break;
                case WildSpawnType.cursedAssault:
                    baseBrain = new GClass322(botOwner);
                    break;
                case WildSpawnType.bossGluhar:
                    baseBrain = new GClass338(botOwner);
                    break;
                case WildSpawnType.followerGluharAssault:
                    baseBrain = new GClass339(botOwner);
                    break;
                case WildSpawnType.followerGluharSecurity:
                case WildSpawnType.followerGluharSnipe:
                    baseBrain = new GClass340(botOwner);
                    break;
                case WildSpawnType.followerGluharScout:
                    baseBrain = new GClass341(botOwner);
                    break;
                case WildSpawnType.followerSanitar:
                    baseBrain = new GClass333(botOwner);
                    break;
                case WildSpawnType.bossSanitar:
                    baseBrain = new GClass317(botOwner);
                    break;
                case WildSpawnType.assaultGroup:
                    baseBrain = new GClass311(botOwner, true);
                    break;
                case WildSpawnType.sectantWarrior:
                    baseBrain = new GClass360(botOwner);
                    break;
                case WildSpawnType.sectantPriest:
                    baseBrain = new GClass362(botOwner);
                    break;
                case WildSpawnType.bossTagilla:
                case WildSpawnType.infectedTagilla:
                    baseBrain = new GClass319(botOwner);
                    break;
                case WildSpawnType.followerTagilla:
                    baseBrain = new GClass334(botOwner);
                    break;
                case WildSpawnType.exUsec:
                    baseBrain = new ExUsecBrainClass(botOwner);
                    break;
                case WildSpawnType.gifter:
                    baseBrain = new GClass336(botOwner);
                    break;
                case WildSpawnType.bossKnight:
                    baseBrain = new BossKnightBrainClass(botOwner);
                    break;
                case WildSpawnType.followerBigPipe:
                    baseBrain = new GClass329(botOwner);
                    break;
                case WildSpawnType.followerBirdEye:
                    baseBrain = new GClass330(botOwner);
                    break;
                case WildSpawnType.bossZryachiy:
                    baseBrain = new GClass321(botOwner);
                    break;
                case WildSpawnType.followerZryachiy:
                    baseBrain = new GClass335(botOwner);
                    break;
                case WildSpawnType.bossBoar:
                    baseBrain = new GClass312(botOwner);
                    break;
                case WildSpawnType.followerBoar:
                    baseBrain = new GClass331(botOwner, false);
                    break;
                case WildSpawnType.arenaFighter:
                    baseBrain = new GClass310(botOwner);
                    break;
                case WildSpawnType.bossBoarSniper:
                    baseBrain = new GClass313(botOwner);
                    break;
                case WildSpawnType.crazyAssaultEvent:
                    baseBrain = new GClass325(botOwner);
                    break;
                case WildSpawnType.peacefullZryachiyEvent:
                    baseBrain = new GClass347(botOwner);
                    break;
                case WildSpawnType.sectactPriestEvent:
                    baseBrain = new GClass352(botOwner);
                    break;
                case WildSpawnType.ravangeZryachiyEvent:
                    baseBrain = new GClass351(botOwner);
                    break;
                case WildSpawnType.followerBoarClose1:
                case WildSpawnType.followerBoarClose2:
                    baseBrain = new GClass331(botOwner, true);
                    break;
                case WildSpawnType.bossKolontay:
                    baseBrain = new GClass343(botOwner);
                    break;
                case WildSpawnType.followerKolontayAssault:
                    baseBrain = new GClass344(botOwner);
                    break;
                case WildSpawnType.followerKolontaySecurity:
                    baseBrain = new GClass345(botOwner);
                    break;
                case WildSpawnType.shooterBTR:
                    baseBrain = new GClass354(botOwner);
                    break;
                case WildSpawnType.bossPartisan:
                    baseBrain = new GClass316(botOwner);
                    break;
                case WildSpawnType.pmcBEAR:
                    baseBrain = new GClass348(botOwner, false);
                    break;
                case WildSpawnType.pmcUSEC:
                    baseBrain = new GClass350(botOwner, false);
                    break;
                case WildSpawnType.sectantPredvestnik:
                    baseBrain = new GClass361(botOwner);
                    break;
                case WildSpawnType.sectantPrizrak:
                    baseBrain = new GClass363(botOwner);
                    break;
                case WildSpawnType.sectantOni:
                    baseBrain = new GClass353(botOwner);
                    break;
                case WildSpawnType.infectedAssault:
                case WildSpawnType.infectedPmc:
                case WildSpawnType.infectedCivil:
                case WildSpawnType.infectedLaborant:
                    baseBrain = new GClass324(botOwner);
                    break;
                case WildSpawnType.bossTagillaAgro:
                    baseBrain = new GClass318(botOwner);
                    break;
                case WildSpawnType.bossKillaAgro:
                    baseBrain = new GClass315(botOwner);
                    break;
                case WildSpawnType.tagillaHelperAgro:
                    baseBrain = new GClass364(botOwner);
                    break;
                default:
                    baseBrain = new GClass355(botOwner);
                    break;
            }

            return baseBrain;
        }
    }
}
