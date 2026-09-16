using MoreBotsServer.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using System;
using System.Collections.Generic;

namespace MoreBotsServer.Interop;

[Injectable(
    InjectionType.Singleton,
    TypePriority = OnLoadOrder.Preload + 1)]
public sealed class SainInteropRegistration
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
                "A SAIN-compatible bot registration must have a valid Name.",
                nameof(registration));
        }

        if (string.IsNullOrWhiteSpace(registration.BotDbKey))
        {
            throw new ArgumentException(
                "A SAIN-compatible bot registration must have a valid BotDbKey.",
                nameof(registration));
        }

        if (registration.BrainsToApply is null ||
            registration.BrainsToApply.Count == 0)
        {
            throw new ArgumentException(
                "A SAIN-compatible bot registration must specify at least one brain.",
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
}