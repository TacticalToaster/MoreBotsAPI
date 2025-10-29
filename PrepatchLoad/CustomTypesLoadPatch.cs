using BepInEx.Logging;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MoreBotsAPI.Prepatch
{
    public static class CustomTypesLoadPatch
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static AssemblyDefinition patchAssembly;

        public static void Patch(AssemblyDefinition assembly)
        {
            patchAssembly = assembly;

            if (!ShouldPatchAssembly())
            {
                Logger.CreateLogSource("MoreBotsAPI Prepatch Loader")
                      .LogWarning("MoreBotsAPI plugin not detected, not patching assembly. Make sure you have installed or uninstalled the mod correctly.");
                return;
            }
            else
            {
                Logger.CreateLogSource("MoreBotsAPI Prepatch Loader")
                      .LogInfo("MoreBotsAPI plugin detected, patching assembly to load custom wild spawn types.");
            }

            var wildSpawnType = patchAssembly.MainModule.GetType("EFT.WildSpawnType");

            foreach (var botType in CustomWildSpawnTypeManager.GetCustomWildSpawnTypes())
            {
                Utils.AddEnumValue(ref wildSpawnType, botType.WildSpawnTypeName, botType.WildSpawnTypeValue);
            }
        }

        private static bool ShouldPatchAssembly()
        {
            var patcherLoc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var bepDir = Directory.GetParent(patcherLoc);
            var modDllLoc = Path.Combine(bepDir.FullName, "plugins", "MoreBotsAPI", "MoreBotsPlugin.dll");

            return File.Exists(modDllLoc);
        }
    }
}
