using SPTarkov.Server.Core.Models.Eft.Common;

public class Faction
{
    public string Name { get; set; } = string.Empty;
    public List<WildSpawnType> BotTypes { get; set; } = new();
    public List<Faction> SubFactions { get; set; } = new();

    public Faction() { }

    public List<WildSpawnType> GetAllBotTypes()
    {
        var allBotTypes = new List<WildSpawnType>(BotTypes);
        foreach (var subFaction in SubFactions)
        {
            allBotTypes.AddRange(subFaction.GetAllBotTypes());
        }
        return allBotTypes.Distinct().ToList();
    }

    public void AddFromFaction(Faction faction)
    {
        foreach (var botType in faction.BotTypes)
        {
            if (!BotTypes.Contains(botType))
            {
                BotTypes.Add(botType);
            }
        }
    }
}