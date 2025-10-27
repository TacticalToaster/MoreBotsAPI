using BepInEx.Bootstrap;
using EFT;
using EFT.InputSystem;
using MoreBotsAPI.Interop;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MoreBotsAPI.Patches
{
    public class TarkovInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.Init), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(IAssetsManager assetsManager, InputTree inputTree)
        {
            bool sainLoaded = Chainloader.PluginInfos.ContainsKey("me.sol.sain");

            if (sainLoaded)
                SAINInterop.AddSAINLayers();
        }
    }
}
