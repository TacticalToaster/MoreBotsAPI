using HarmonyLib.Tools;
using MoreBotsServer.Models;
using SAIN.ServerInterop;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using System;
using System.Collections.Generic;

namespace MoreBotsServer.Interop;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.Preload + 1)]
public sealed class SainInteropRegistration(ISptLogger<SainInteropRegistration> logger)
{
    private readonly Dictionary<int, MoreBotsSainBotTypeRegistration>
        _customWildSpawnTypes = [];

    private readonly Dictionary<string, MoreBotsSainBotTypeRegistration>
        _customTypeNames =
            new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, MoreBotsSainBotTypeRegistration>
        _customTypeDbKeys =
            new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<MoreBotsSainBotTypeRegistration> Registrations =>
        _customWildSpawnTypes.Values;

    public void RegisterBotType(MoreBotsSainBotTypeRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        if (string.IsNullOrWhiteSpace(registration.Name))
        {
            throw new ArgumentException(
                "A SAIN bot registration must have a valid Name.",
                nameof(registration));
        }

        if (string.IsNullOrWhiteSpace(registration.BotDbKey))
        {
            throw new ArgumentException(
                "A SAIN bot registration must have a valid BotDbKey.",
                nameof(registration));
        }

        if (registration.BrainsToApply is null ||
            registration.BrainsToApply.Count == 0)
        {
            throw new ArgumentException(
                "A SAIN bot registration must specify at least one brain.",
                nameof(registration));
        }

        if (_customWildSpawnTypes.TryGetValue(
                registration.WildSpawnType,
                out var existingWildSpawnType))
        {
            throw new InvalidOperationException(
                $"Cannot register SAIN bot '{registration.Name}': " +
                $"WildSpawnType {registration.WildSpawnType} is already registered " +
                $"by '{existingWildSpawnType.Name}'.");
        }

        if (_customTypeNames.TryGetValue(
                registration.Name,
                out var existingName))
        {
            throw new InvalidOperationException(
                $"Cannot register SAIN bot '{registration.Name}': " +
                $"Name is already registered for WildSpawnType " +
                $"{existingName.WildSpawnType}.");
        }

        if (_customTypeDbKeys.TryGetValue(
                registration.BotDbKey,
                out var existingBotDbKey))
        {
            throw new InvalidOperationException(
                $"Cannot register SAIN bot '{registration.Name}': " +
                $"BotDbKey '{registration.BotDbKey}' is already registered " +
                $"by '{existingBotDbKey.Name}' " +
                $"(WildSpawnType {existingBotDbKey.WildSpawnType}).");
        }

        _customWildSpawnTypes.Add(registration.WildSpawnType, registration);
        _customTypeNames.Add(registration.Name, registration);
        _customTypeDbKeys.Add(registration.BotDbKey, registration);
    }

    [Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
    internal sealed class SainInteropPostLoad(
        ISptLogger<SainInteropPostLoad> logger,
        IServiceProvider services,
        SainInteropRegistration sainInterop) : IOnLoad
    {
        public async Task OnLoadAsync(
            CancellationToken cancellationToken = default)
        {
            var sain = services.GetService<ISainBotTypeRegistry>();

            if (sain is null)
            {
                logger.Warning(
                    "SAIN server interop was not found; no custom SAIN bot types were registered.");

                return;
            }
            var count = 0;

            foreach (var registration in sainInterop.Registrations)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await sain.RegisterAsync(
                    new SainBotTypeRegistration
                    {
                        Name = registration.Name,
                        WildSpawnType = registration.WildSpawnType,
                        BotDbKey = registration.BotDbKey,
                        Section = registration.Section,
                        Description = registration.Description,
                        DifficultyModifier = registration.DifficultyModifier,
                        BaseBrain = registration.BaseBrain,
                        BrainsToApply = [.. registration.BrainsToApply],
                        LayersToRemove = registration.LayersToRemove is null
                            ? null
                            : [.. registration.LayersToRemove],
                    },
                    cancellationToken);

                count++;
            }
        }
    }
}