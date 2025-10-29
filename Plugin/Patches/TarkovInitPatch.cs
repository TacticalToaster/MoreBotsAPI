using BepInEx.Bootstrap;
using EFT;
using EFT.InputSystem;
using MoreBotsAPI.Interop;
using SPT.Reflection.Patching;
using System.Reflection;

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
            {
                Logger.LogMessage("SAIN detected, initializing SAIN interop for MoreBotsAPI.");
                new SAINInterop().Init();
                SAINInterop.AddSAINLayers();
            }
        }
    }
}
