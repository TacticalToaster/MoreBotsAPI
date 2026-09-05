using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.DI;

namespace MoreBotsServer;

public static class MoreBotsLoadOrder
{
    public const int LoadFactions = OnLoadOrder.Preload + 80080;

    public const int LoadBots = OnLoadOrder.Preload + 80085;
}