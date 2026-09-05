using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using static UnityEngine.UI.GridLayoutGroup;

namespace MoreBotsAPI.Patches
{
    public class StandartBotBrainActivatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(StandartBotBrain), nameof(StandartBotBrain.Activate));
        }

        [PatchPostfix]
        [HarmonyPriority(Priority.First)] // Make sure this runs before BigBrain so we can override it
        public static void PatchPrefix(StandartBotBrain __instance, BotOwner ____owner)
        {
            try
            {
                if (CustomWildSpawnTypeManager.IsCustomWildSpawnType((int)____owner.Profile.Info.Settings.Role))
                {
                    __instance.BaseBrain?.Dispose();
                    __instance.Agent?.Dispose();

                    var customType = ____owner.Profile.Info.Settings.Role.GetCustomType();
                    Logger.LogMessage($"Changing brain for custom bot {____owner.Profile.Info.Settings.Role}.");
                    __instance.BaseBrain = GetBaseBrain(____owner, customType.BaseBrain);
                    __instance.Agent = GetAgent(____owner, __instance.BaseBrain, __instance);

                    var eventDelegate = (MulticastDelegate)
                        typeof(StandartBotBrain).GetField(nameof(StandartBotBrain.OnSetStrategy), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

                    if (eventDelegate != null)
                    {
                        foreach (var handler in eventDelegate.GetInvocationList())
                        {
                            handler.Method.Invoke(handler.Target, [__instance.BaseBrain]);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Custom bot API ran into an error when checking/assigning brain to custom bot type: {e.Message}");
            }
        }

        public static AICoreAgent<BotLogicDecision> GetAgent(BotOwner botOwner, BaseBrain baseBrain, StandartBotBrain standartBotBrain)
        {
            var name = botOwner.name + " " + botOwner.Profile.Info.Settings.Role.ToString();
            return new AICoreAgent<BotLogicDecision>(botOwner.BotsController.AICoreController, baseBrain, AIActionsList.ActionsList(botOwner), botOwner.gameObject, name, new Func<BotLogicDecision, AICoreNode>(standartBotBrain.CG_Activate));
        }

        public static BaseBrain GetBaseBrain(BotOwner botOwner, int brainType)
        {
            BaseBrain baseBrain;

            switch ((WildSpawnType)brainType)
            {
                case WildSpawnType.marksman:
                    baseBrain = new MarksmanLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossTest:
                    baseBrain = new BossTestLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossBully:
                    baseBrain = new BossBullyLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerBully:
                    baseBrain = new FollowerBullyLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossKilla:
                    baseBrain = new KillaLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossKojaniy:
                    baseBrain = new BossKojaniyLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerKojaniy:
                    baseBrain = new FollowerKojaniyLayersStrategy(botOwner);
                    break;
                case WildSpawnType.pmcBot:
                case WildSpawnType.arenaFighterEvent:
                    baseBrain = new PmcLayersStrategy(botOwner, false);
                    break;
                case WildSpawnType.cursedAssault:
                    baseBrain = new CursedAssaultLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossGluhar:
                    baseBrain = new BossGluharLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerGluharAssault:
                    baseBrain = new FollowerGluharAssaultLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerGluharSecurity:
                case WildSpawnType.followerGluharSnipe:
                    baseBrain = new FollowerGluharProtectLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerGluharScout:
                    baseBrain = new FollowerGluharScoutLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerSanitar:
                    baseBrain = new FollowerSanitarLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossSanitar:
                    baseBrain = new BossSanitarLayersStrategy(botOwner);
                    break;
                case WildSpawnType.assaultGroup:
                    baseBrain = new AssaultGroupLayersStrategy(botOwner, true);
                    break;
                case WildSpawnType.sectantWarrior:
                    baseBrain = new SectantWarriorLayersStrategy(botOwner);
                    break;
                case WildSpawnType.sectantPriest:
                    baseBrain = new SectantPriestLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossTagilla:
                case WildSpawnType.infectedTagilla:
                    baseBrain = new BossTagillaLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerTagilla:
                    baseBrain = new FollowerTagillaLayersStrategy(botOwner);
                    break;
                case WildSpawnType.exUsec:
                    baseBrain = new ExUsecLayersStrategy(botOwner);
                    break;
                case WildSpawnType.gifter:
                    baseBrain = new GifterLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossKnight:
                    baseBrain = new BossKnightLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerBigPipe:
                    baseBrain = new FollowerBigPipeLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerBirdEye:
                    baseBrain = new FollowerBirdEyeLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossZryachiy:
                    baseBrain = new BossZryachiyLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerZryachiy:
                    baseBrain = new FollowerZryachiyLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossBoar:
                    baseBrain = new BossBoarLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerBoar:
                    baseBrain = new FollowerBoarLayersStrategy(botOwner, false);
                    break;
                case WildSpawnType.arenaFighter:
                    baseBrain = new ArenaFighterLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossBoarSniper:
                    baseBrain = new BossBoarLayersStrategy(botOwner);
                    break;
                case WildSpawnType.crazyAssaultEvent:
                    baseBrain = new ObdolbosLayersStrategy(botOwner);
                    break;
                case WildSpawnType.peacefullZryachiyEvent:
                    baseBrain = new PeacefullZryachiyEventLayersStrategy(botOwner);
                    break;
                case WildSpawnType.sectactPriestEvent:
                    baseBrain = new SectactPriestEventStrategy(botOwner);
                    break;
                case WildSpawnType.ravangeZryachiyEvent:
                    baseBrain = new RavangeZryachiyEventLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerBoarClose1:
                case WildSpawnType.followerBoarClose2:
                    baseBrain = new FollowerBoarLayersStrategy(botOwner, true);
                    break;
                case WildSpawnType.bossKolontay:
                    baseBrain = new BossKolontayLayersStrategy(botOwner);
                    break;
                case WildSpawnType.followerKolontayAssault:
                    baseBrain = new FollowerKolontayAssaultStrategy(botOwner);
                    break;
                case WildSpawnType.followerKolontaySecurity:
                    baseBrain = new FollowerKolontaySecutiryLayersStrategy(botOwner);
                    break;
                case WildSpawnType.shooterBTR:
                    baseBrain = new ShooterBTRLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossPartisan:
                    baseBrain = new BossPartisanLayersStrategy(botOwner);
                    break;
                case WildSpawnType.pmcBEAR:
                    baseBrain = new PmcBearLayersStrategy(botOwner, false);
                    break;
                case WildSpawnType.pmcUSEC:
                    baseBrain = new PmcUsecLayersStrategy(botOwner, false);
                    break;
                case WildSpawnType.sectantPredvestnik:
                    baseBrain = new SectantPredvestnikLayersStrategy(botOwner);
                    break;
                case WildSpawnType.sectantPrizrak:
                    baseBrain = new SectantPrizrakLayersStrategy(botOwner);
                    break;
                case WildSpawnType.sectantOni:
                    baseBrain = new SectantOniLayersStrategy(botOwner);
                    break;
                case WildSpawnType.infectedAssault:
                case WildSpawnType.infectedPmc:
                case WildSpawnType.infectedCivil:
                case WildSpawnType.infectedLaborant:
                    baseBrain = new InfectedLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossTagillaAgro:
                    baseBrain = new BossTagillaAgroLayersStrategy(botOwner);
                    break;
                case WildSpawnType.bossKillaAgro:
                    baseBrain = new BossKillaAgroLayersStrategy(botOwner);
                    break;
                case WildSpawnType.tagillaHelperAgro:
                    baseBrain = new TagillaHelperAgroStrategy(botOwner);
                    break;
                default:
                    baseBrain = new StandardAssaultLayersStrategy(botOwner);
                    break;
            }

            return baseBrain;
        }
    }
}
