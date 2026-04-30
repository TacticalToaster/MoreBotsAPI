using System;
using System.Reflection;

namespace MoreBotsAPI.Interop;

public static class FikaInterop
{
    private static Assembly FikaAssembly { get; set; }
    private static Type FikaBackendUtils { get; set; }
    private static PropertyInfo IsServerProperty { get; set; }
    private static PropertyInfo IsClientProperty { get; set; }

    public static bool FikaExists = false;

    public static void InitializeInterop()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == "Fika.Core")
            {
                FikaAssembly = assembly;
                break;
            }
        }

        if (FikaAssembly == null)
        {
            return;
        }
        
        FikaExists = true;

        FikaBackendUtils = FikaAssembly.GetType("Fika.Core.Main.Utils.FikaBackendUtils");
        IsServerProperty = FikaBackendUtils.GetProperty("IsServer", BindingFlags.Public | BindingFlags.Static);
        IsClientProperty = FikaBackendUtils.GetProperty("IsClient", BindingFlags.Public | BindingFlags.Static);
    }

    public static bool IsServer()
    {
        if (IsServerProperty == null)
        {
            return false;
        }

        return (bool)IsServerProperty.GetValue(null);
    }

    public static bool IsClient()
    {
        if (IsClientProperty == null)
        {
            return false;
        }

        return (bool)IsClientProperty.GetValue(null);
    }
}