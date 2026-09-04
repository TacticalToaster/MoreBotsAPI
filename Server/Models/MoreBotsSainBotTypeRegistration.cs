using System.Collections.Generic;

namespace MoreBotsServer.Models;

public record MoreBotsSainBotTypeRegistration
{
    public required string Name { get; init; }

    public required int WildSpawnType { get; init; }

    public required string BotDbKey { get; init; }

    public string Section { get; init; } = "Modded";

    public string? Description { get; init; }

    public float DifficultyModifier { get; init; } = 0.5f;

    public string? BaseBrain { get; init; }

    public required IReadOnlyCollection<string> BrainsToApply { get; init; }

    public IReadOnlyCollection<string>? LayersToRemove { get; init; }
}