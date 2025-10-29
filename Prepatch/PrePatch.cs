using BepInEx;

namespace MoreBotsAPI.Prepatch
{
    [BepInPlugin(ClientInfo.PreLoadGUID, ClientInfo.PreLoadName, ClientInfo.Version)]
    public class MoreBotsPrepatch : BaseUnityPlugin
    {
        public static MoreBotsPrepatch Instance;

        public void Awake()
        {
            Instance = this;
        }
    }
}
