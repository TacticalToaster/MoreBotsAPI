using System.Collections.Generic;

namespace MoreBotsAPI
{
    public class SAINSettings
    {
        // Int value for your custom bot type enum
        public int WildSpawnType;
        // Name you want to show in the SAIN settings
        public string Name = "Default";
        // Description to show in SAIN settings for your bot
        public string Description = "Default Description";
        // Section your bot shows up in SAIN settings. Examples include Bosses, Followers, PMCs, or you can set your own.
        public string Section = "Default Section";
        // Not used unless BrainToApply is null
        public string BaseBrain = "DefaultBrain";
        // Bot brains to apply SAIN layers to
        public List<string> BrainsToApply;
        // Layers to remove from the brains above, to prevent default EFT behaviors from interferring with SAIN. Typically, you'd remove combat layers here.
        public List<string> LayersToRemove;

        public SAINSettings(int wildSpawnType)
        {
            WildSpawnType = wildSpawnType;
        }
    }
}
