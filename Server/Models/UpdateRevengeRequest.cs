using SPTarkov.Server.Core.Models.Utils;

namespace MoreBotsServer.Models;

public record UpdateRevengeRequest : IRequestData
{
    public Dictionary<string, List<string>>? RevengeUpdate { get; set; } = new();
}