using HarmonyLib;
using MoreBotsServer.Interop;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using System.Collections.Generic;
using System.Reflection;

namespace MoreBotsServer.Interop;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
internal sealed class SainBotRegistrationBridge(
    ISptLogger<SainBotRegistrationBridge> logger,
    IServiceProvider services,
    SainInteropRegistration registrations) : IOnLoad
{
    public async Task OnLoadAsync(
        CancellationToken cancellationToken = default)
    {
        var registryType = AccessTools.TypeByName(
            "SAIN.ServerInterop.ISainBotTypeRegistry");

        var registrationType = AccessTools.TypeByName(
            "SAIN.ServerInterop.SainBotTypeRegistration");

        if (registryType is null || registrationType is null)
        {
            return;
        }

        var sain = services.GetService(registryType);

        if (sain is null)
        {
            return;
        }

        var registerMethod = AccessTools.Method(
            sain.GetType(),
            "RegisterAsync");

        if (registerMethod is null)
        {
            logger.Warning(
                "SAIN was found, but ISainBotTypeRegistry.RegisterAsync " +
                "could not be located.");

            return;
        }

        var properties = SainRegistrationProperties.Create(
            registrationType);

        if (properties is null)
        {
            logger.Warning(
                "SAIN was found, but SainBotTypeRegistration is missing " +
                "one or more required properties.");

            return;
        }

        var count = 0;

        foreach (var registration in registrations.Registrations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sainRegistration =
                Activator.CreateInstance(registrationType);

            if (sainRegistration is null)
            {
                logger.Warning(
                    "Could not create SAIN SainBotTypeRegistration.");

                return;
            }

            properties.Name.SetValue(
                sainRegistration,
                registration.Name);

            properties.WildSpawnType.SetValue(
                sainRegistration,
                registration.WildSpawnType);

            properties.BotDbKey.SetValue(
                sainRegistration,
                registration.BotDbKey);

            properties.Section.SetValue(
                sainRegistration,
                registration.Section);

            properties.Description.SetValue(
                sainRegistration,
                registration.Description);

            properties.DifficultyModifier.SetValue(
                sainRegistration,
                registration.DifficultyModifier);

            properties.BaseBrain.SetValue(
                sainRegistration,
                registration.BaseBrain);

            properties.BrainsToApply.SetValue(
                sainRegistration,
                new List<string>(registration.BrainsToApply));

            properties.LayersToRemove.SetValue(
                sainRegistration,
                registration.LayersToRemove is null
                    ? null
                    : new List<string>(
                        registration.LayersToRemove));

            var task = (Task?)registerMethod.Invoke(
                sain,
                [sainRegistration, cancellationToken]);

            if (task is null)
            {
                logger.Warning(
                    $"SAIN RegisterAsync returned null for " +
                    $"'{registration.Name}'.");

                continue;
            }

            await task;

            count++;
        }
    }

    private sealed record SainRegistrationProperties(
        PropertyInfo Name,
        PropertyInfo WildSpawnType,
        PropertyInfo BotDbKey,
        PropertyInfo Section,
        PropertyInfo Description,
        PropertyInfo DifficultyModifier,
        PropertyInfo BaseBrain,
        PropertyInfo BrainsToApply,
        PropertyInfo LayersToRemove)
    {
        public static SainRegistrationProperties? Create(
            Type registrationType)
        {
            var name = registrationType.GetProperty("Name");
            var wildSpawnType =
                registrationType.GetProperty("WildSpawnType");
            var botDbKey = registrationType.GetProperty("BotDbKey");
            var section = registrationType.GetProperty("Section");
            var description =
                registrationType.GetProperty("Description");
            var difficultyModifier =
                registrationType.GetProperty("DifficultyModifier");
            var baseBrain =
                registrationType.GetProperty("BaseBrain");
            var brainsToApply =
                registrationType.GetProperty("BrainsToApply");
            var layersToRemove =
                registrationType.GetProperty("LayersToRemove");

            if (name is null ||
                wildSpawnType is null ||
                botDbKey is null ||
                section is null ||
                description is null ||
                difficultyModifier is null ||
                baseBrain is null ||
                brainsToApply is null ||
                layersToRemove is null)
            {
                return null;
            }

            return new SainRegistrationProperties(
                name,
                wildSpawnType,
                botDbKey,
                section,
                description,
                difficultyModifier,
                baseBrain,
                brainsToApply,
                layersToRemove);
        }
    }
}