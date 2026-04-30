using System.Collections.Generic;
using JetBrains.Annotations;

namespace MoreBotsAPI.Models;

public record UpdateRevengeRequest
{
    [CanBeNull] public Dictionary<string, List<string>> RevengeUpdate { get; set; } = new();
}