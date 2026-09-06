using BepInEx.Logging;
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

        public static void Patch(AssemblyDefinition assembly)
        {
            patchAssembly = assembly;

            if (!ShouldPatchAssembly())
            {
                Logger.CreateLogSource("MoreBotsAPI Prepatch")
                      .LogWarning("MoreBotsAPI plugin not detected, not patching assembly. Make sure you have installed or uninstalled the mod correctly.");
                return;
            }

            //CustomWildSpawnTypeManager.AddType("CustomBotType1", "custom_role_1", 1);
        }

        private static bool ShouldPatchAssembly()
        {
            var patcherLoc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrEmpty(patcherLoc))
                return false;

            var nestedDir = Directory.GetParent(patcherLoc);
            var bepDir = nestedDir?.Parent;

            if (bepDir == null)
                return false;

            var modDllLoc = Path.Combine(
                bepDir.FullName,
                "plugins",
                "MoreBotsPlugin",
                "MoreBotsPlugin.dll"
            );

            return File.Exists(modDllLoc);
        }
    }
}
