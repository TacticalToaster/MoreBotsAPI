using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace MoreBotsServer.Services;

// Hacky ass solution to handle this, but I dont see any other way to do this without recompiling all the more bots packs
[Injectable(InjectionType.Singleton)]
internal sealed class LoadBotsSpawnRecord(LocationTable locationTable, MoreBotsLogger logger)
{
    private readonly Dictionary<string, HashSet<BossLocationSpawn>> _before = new();
    private readonly Dictionary<string, MapChanges> _changes = new();

    public void CaptureBefore()
    {
        foreach (var (key, location) in locationTable.GetDictionary())
        {
            _before[key] = ToReferenceSet(location.Base?.BossLocationSpawn);
        }
    }

    public void CaptureAfter()
    {
        foreach (var (key, location) in locationTable.GetDictionary())
        {
            var after = location.Base?.BossLocationSpawn ?? [];
            var before = _before.GetValueOrDefault(key) ?? ToReferenceSet(null);
            var afterSet = ToReferenceSet(after);

            var added = after.Where(spawn => !before.Contains(spawn)).ToList();

            if (added.Count == 0)
            {
                continue;
            }

            var removed = before
                .Where(spawn => !afterSet.Contains(spawn))
                .Select(spawn => (spawn.BossName, spawn.TriggerId))
                .ToHashSet();

            _changes[key] = new MapChanges(added, removed);
        }

        _before.Clear();
    }

    public void Restore()
    {
        var locations = locationTable.GetDictionary();

        foreach (var (key, changes) in _changes)
        {
            var locationBase = locations[key].Base;
            locationBase.BossLocationSpawn ??= [];
            var current = locationBase.BossLocationSpawn;
            var currentSet = ToReferenceSet(current);

            var missing = changes.Added.Where(spawn => !currentSet.Contains(spawn)).ToList();

            if (missing.Count == 0)
            {
                continue;
            }
            
            var addedSet = ToReferenceSet(changes.Added);
            current.RemoveAll(spawn => !addedSet.Contains(spawn) && changes.Removed.Contains((spawn.BossName, spawn.TriggerId)));
            current.AddRange(missing);

            logger.Info($"Reapplied {missing.Count} LoadBots spawns on {key} after the spawn list was replaced.");
        }

        _changes.Clear();
    }

    private static HashSet<BossLocationSpawn> ToReferenceSet(IEnumerable<BossLocationSpawn>? spawns)
    {
        return new HashSet<BossLocationSpawn>(spawns ?? [], ReferenceEqualityComparer.Instance);
    }

    private sealed record MapChanges(List<BossLocationSpawn> Added, HashSet<(string? BossName, string? TriggerId)> Removed);
}

[Injectable(TypePriority = MoreBotsLoadOrder.LoadBots - 1)]
internal sealed class LoadBotsSpawnCaptureBefore(LoadBotsSpawnRecord record) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        record.CaptureBefore();
        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = MoreBotsLoadOrder.LoadBots + 1)]
internal sealed class LoadBotsSpawnCaptureAfter(LoadBotsSpawnRecord record) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        record.CaptureAfter();
        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = MoreBotsLoadOrder.AddSpawns)]
internal sealed class LoadBotsSpawnRestore(LoadBotsSpawnRecord record) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        record.Restore();
        return Task.CompletedTask;
    }
}
