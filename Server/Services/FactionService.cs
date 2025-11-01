using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace MoreBotsServer.Services;

[Injectable(InjectionType.Singleton)]
public class FactionService(
    ISptLogger<MoreBotsCustomBotTypeService> logger,
    MoreBotsCustomBotTypeService customBotTypeService,
    DatabaseService databaseService
)
{
    public Dictionary<string, Faction> Factions { get; } = new();

    public void InitFactions()
    {
        LoadDefaultFactions();
    }

    public Dictionary<string, Faction> GetFactions()
    {
        return Factions;
    }

    public void AddEnemyByFaction(BotType botType, string factionName)
    {
        if (Factions.TryGetValue(factionName, out var faction))
        {
            var enemyBotTypes = faction.GetAllBotTypes();
            botType?.BotDifficulty["easy"]?.Mind?.EnemyBotTypes?.AddRange(enemyBotTypes);
            botType?.BotDifficulty["normal"]?.Mind?.EnemyBotTypes?.AddRange(enemyBotTypes);
            botType?.BotDifficulty["hard"]?.Mind?.EnemyBotTypes?.AddRange(enemyBotTypes);
            botType?.BotDifficulty["impossible"]?.Mind?.EnemyBotTypes?.AddRange(enemyBotTypes);
        }
        else
        {
            logger.Warning($"Faction '{factionName}' not found when setting enemies for bot type '{botType}'.");
        }
    }

    public void AddEnemyByFaction(IEnumerable<string> types, string factionName)
    {
        foreach (var type in types)
        {
            if (databaseService.GetBots().Types.TryGetValue(type, out var botType))
            {
                AddEnemyByFaction(botType, factionName);
            }
            else
            {
                logger.Warning($"Bot type '{type}' not found when setting enemies by faction '{factionName}'.");
            }
        }
    }

    public void AddFriendlyByFaction(BotType botType, string factionName)
    {
        if (Factions.TryGetValue(factionName, out var faction))
        {
            var allyBotTypes = faction.GetAllBotTypes();
            botType?.BotDifficulty["easy"]?.Mind?.FriendlyBotTypes?.AddRange(allyBotTypes);
            botType?.BotDifficulty["normal"]?.Mind?.FriendlyBotTypes?.AddRange(allyBotTypes);
            botType?.BotDifficulty["hard"]?.Mind?.FriendlyBotTypes?.AddRange(allyBotTypes);
            botType?.BotDifficulty["impossible"]?.Mind?.FriendlyBotTypes?.AddRange(allyBotTypes);
        }
        else
        {
            logger.Warning($"Faction '{factionName}' not found when setting friendlies for bot type '{botType}'.");
        }
    }

    public void AddFriendlyByFaction(IEnumerable<string> types, string factionName)
    {
        foreach (var type in types)
        {
            if (databaseService.GetBots().Types.TryGetValue(type, out var botType))
            {
                AddFriendlyByFaction(botType, factionName);
            }
            else
            {
                logger.Warning($"Bot type '{type}' not found when setting friendlies by faction '{factionName}'.");
            }
        }
    }

    public void AddWarnByFaction(BotType botType, string factionName)
    {
        if (Factions.TryGetValue(factionName, out var faction))
        {
            var warnBotTypes = faction.GetAllBotTypes();
            botType?.BotDifficulty["easy"]?.Mind?.WarnBotTypes?.AddRange(warnBotTypes);
            botType?.BotDifficulty["normal"]?.Mind?.WarnBotTypes?.AddRange(warnBotTypes);
            botType?.BotDifficulty["hard"]?.Mind?.WarnBotTypes?.AddRange(warnBotTypes);
            botType?.BotDifficulty["impossible"]?.Mind?.WarnBotTypes?.AddRange(warnBotTypes);
        }
        else
        {
            logger.Warning($"Faction '{factionName}' not found when setting warns for bot type '{botType}'.");
        }
    }

    public void AddWarnByFaction(IEnumerable<string> types, string factionName)
    {
        foreach (var type in types)
        {
            if (databaseService.GetBots().Types.TryGetValue(type, out var botType))
            {
                AddWarnByFaction(botType, factionName);
            }
            else
            {
                logger.Warning($"Bot type '{type}' not found when setting warns by faction '{factionName}'.");
            }
        }
    }

    public void AddRevengeByFaction(BotType botType, string factionName)
    {
        if (Factions.TryGetValue(factionName, out var faction))
        {
            var revengeBotTypes = faction.GetAllBotTypes();
            botType?.BotDifficulty["easy"]?.Mind?.RevengeBotTypes?.AddRange(revengeBotTypes);
            botType?.BotDifficulty["normal"]?.Mind?.RevengeBotTypes?.AddRange(revengeBotTypes);
            botType?.BotDifficulty["hard"]?.Mind?.RevengeBotTypes?.AddRange(revengeBotTypes);
            botType?.BotDifficulty["impossible"]?.Mind?.RevengeBotTypes?.AddRange(revengeBotTypes);
        }
        else
        {
            logger.Warning($"Faction '{factionName}' not found when setting revenge for bot type '{botType}'.");
        }
    }

    public void AddRevengeByFaction(IEnumerable<string> types, string factionName)
    {
        foreach (var type in types)
        {
            if (databaseService.GetBots().Types.TryGetValue(type, out var botType))
            {
                AddRevengeByFaction(botType, factionName);
            }
            else
            {
                logger.Warning($"Bot type '{type}' not found when setting revenge by faction '{factionName}'.");
            }
        }
    }

    public void LoadDefaultFactions()
    {
        var raiders = new Faction
        {
            Name = "raiders",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.pmcBot
            }
        };

        Factions.Add(raiders.Name, raiders);

        var rogues = new Faction
        {
            Name = "rogues",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.exUsec,
                WildSpawnType.bossKnight,
                WildSpawnType.followerBigPipe,
                WildSpawnType.followerBirdEye
            }
        };

        Factions.Add(rogues.Name, rogues);

        var smugglers = new Faction
        {
            Name = "smugglers",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.arenaFighterEvent
            }
        };

        Factions.Add(smugglers.Name, smugglers);

        var bloodhounds = new Faction
        {
            Name = "bloodhounds",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.arenaFighter
            }
        };

        Factions.Add(bloodhounds.Name, bloodhounds);

        var scavs = new Faction
        {
            Name = "scavs",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.assault,
                WildSpawnType.assaultGroup,
                WildSpawnType.cursedAssault,
                WildSpawnType.marksman,
                WildSpawnType.crazyAssaultEvent,
                WildSpawnType.spiritSpring,
                WildSpawnType.spiritWinter,
                WildSpawnType.skier,
                WildSpawnType.peacemaker
            }
        };

        Factions.Add(scavs.Name, scavs);

        var cultists = new Faction
        {
            Name = "cultists",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.sectantWarrior,
                WildSpawnType.sectantPriest,
                WildSpawnType.sectantOni,
                WildSpawnType.sectantPrizrak,
                WildSpawnType.sectantPredvestnik,
                WildSpawnType.bossZryachiy,
                WildSpawnType.followerZryachiy,
                WildSpawnType.peacefullZryachiyEvent,
                WildSpawnType.ravangeZryachiyEvent,
                WildSpawnType.sectactPriestEvent
            }
        };

        Factions.Add(cultists.Name, cultists);

        var infected = new Faction
        {
            Name = "infected",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.infectedAssault,
                WildSpawnType.infectedCivil,
                WildSpawnType.infectedLaborant,
                WildSpawnType.infectedPmc,
                WildSpawnType.infectedTagilla
            }
        };

        Factions.Add(infected.Name, infected);

        var usec = new Faction
        {
            Name = "usec",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.pmcUSEC
            }
        };

        Factions.Add(usec.Name, usec);

        var bear = new Faction
        {
            Name = "bear",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.pmcBEAR
            }
        };

        Factions.Add(bear.Name, bear);

        var pmcs = new Faction
        {
            Name = "pmcs",
            SubFactions = new List<Faction>
            {
                usec,
                bear
            }
        };

        Factions.Add(pmcs.Name, pmcs);

        var killaTagilla = new Faction
        {
            Name = "killaTagilla",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.bossKilla,
                WildSpawnType.bossTagilla,
                WildSpawnType.followerTagilla,
                WildSpawnType.bossTagillaAgro,
                WildSpawnType.tagillaHelperAgro,
                WildSpawnType.bossKillaAgro
            }
        };

        Factions.Add(killaTagilla.Name, killaTagilla);

        var kabanKolontay = new Faction
        {
            Name = "kabanKolontay",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.bossBoar,
                WildSpawnType.bossBoarSniper,
                WildSpawnType.followerBoar,
                WildSpawnType.followerBoarClose1,
                WildSpawnType.followerBoarClose2,
                WildSpawnType.bossKolontay,
                WildSpawnType.followerKolontayAssault,
                WildSpawnType.followerKolontaySecurity
            }
        };

        Factions.Add(kabanKolontay.Name, kabanKolontay);

        var reshala = new Faction
        {
            Name = "reshala",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.bossBully,
                WildSpawnType.followerBully
            }
        };

        Factions.Add(reshala.Name, reshala);

        var shturman = new Faction
        {
            Name = "shturman",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.bossKojaniy,
                WildSpawnType.followerKojaniy
            }
        };

        Factions.Add(shturman.Name, shturman);

        var gluhar = new Faction
        {
            Name = "gluhar",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.bossGluhar,
                WildSpawnType.followerGluharAssault,
                WildSpawnType.followerGluharSnipe,
                WildSpawnType.followerGluharSecurity,
                WildSpawnType.followerGluharScout
            }
        };

        Factions.Add(gluhar.Name, gluhar);

        var sanitar = new Faction
        {
            Name = "sanitar",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.bossSanitar,
                WildSpawnType.followerSanitar
            }
        };

        Factions.Add(sanitar.Name, sanitar);

        var partisan = new Faction
        {
            Name = "partisan",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.bossPartisan
            }
        };

        Factions.Add(partisan.Name, partisan);

        var misc = new Faction
        {
            Name = "misc",
            BotTypes = new List<WildSpawnType>
            {
                WildSpawnType.shooterBTR,
                WildSpawnType.gifter
            }
        };

        Factions.Add(misc.Name, misc);

        var scavbosses = new Faction
        {
            Name = "scavbosses",
            SubFactions = new List<Faction>
            {
                killaTagilla,
                kabanKolontay,
                reshala,
                shturman,
                gluhar,
                sanitar
            }
        };

        Factions.Add(scavbosses.Name, scavbosses);

        var criminals = new Faction
        {
            Name = "criminals",
            SubFactions = new List<Faction>
            {
                scavs,
                scavbosses
            }
        };

        Factions.Add(criminals.Name, criminals);

        var savage = new Faction
        {
            Name = "savage",
            SubFactions = new List<Faction>
            {
                scavs,
                scavbosses,
                smugglers,
                bloodhounds,
                raiders
            }
        };

        Factions.Add(savage.Name, savage);
    }
}