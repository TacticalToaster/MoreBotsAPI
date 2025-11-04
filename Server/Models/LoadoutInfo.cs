using System.Text.Json.Serialization;

namespace MoreBotsServer.Models
{
    public class LoadoutItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("chance")]
        public int? Chance { get; set; }

        [JsonPropertyName("children")]
        public Dictionary<string, LoadoutItem>? Children { get; set; }

        [JsonPropertyName("slots")]
        public Dictionary<string, List<string>>? Slots { get; set; }
    }

    public class LoadoutInfo
    {
        [JsonPropertyName("equipment")]
        public Dictionary<string, Dictionary<string, LoadoutItem>>? Equipment { get; set; }

        [JsonPropertyName("weapons")]
        public Dictionary<string, Dictionary<string, List<string>>>? Weapons { get; set; }

        [JsonPropertyName("categories")]
        public Dictionary<string, Dictionary<string, LoadoutItem>>? Categories { get; set; }
    }
}