using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
