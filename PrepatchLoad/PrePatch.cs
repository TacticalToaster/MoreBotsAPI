using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreBotsAPI.Prepatch
{
    [BepInPlugin("com.morebotsapiprepatchload.tacticaltoaster", "MoreBotsAPIPrepatchLoad", "1.0.0")]
    [BepInDependency("zzzzzzzzzz.ZZZZZ", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.morebotsapi.tacticaltoaster", BepInDependency.DependencyFlags.HardDependency)]
    public class MoreBotsPrepatchLoad : BaseUnityPlugin
    {
        public static MoreBotsPrepatchLoad Instance;

        public void Awake()
        {
            Instance = this;
        }
    }
}
