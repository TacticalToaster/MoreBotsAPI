using SPTarkov.Server.Core.DI;

namespace MoreBotsServer;

public static class MoreBotsLoadOrder
{
    public const int LoadFactions = OnLoadOrder.PostDBModLoader + 6;

    public const int LoadBots = OnLoadOrder.PostDBModLoader + 7;
}