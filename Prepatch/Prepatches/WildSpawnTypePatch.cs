using BepInEx.Logging;
using EFT;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MoreBotsAPI.Prepatch
{
    public static class WildSpawnTypePatch
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static AssemblyDefinition patchAssembly;

        public static void Patch(ref AssemblyDefinition assembly)
        {
            patchAssembly = assembly;

            if (!ShouldPatchAssembly())
            {
                Logger.CreateLogSource("MoreBotsAPI Prepatch")
                      .LogWarning("MoreBotsAPI plugin not detected, not patching assembly. Make sure you have installed or uninstalled the mod correctly.");
                return;
            }

            CustomWildSpawnTypeManager.AddType("CustomBotType1", "custom_role_1", 1);
        }

        public static void Finish()
        {
            if (!ShouldPatchAssembly()) { return; }

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
