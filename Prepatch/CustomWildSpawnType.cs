
namespace MoreBotsAPI
{
    public class CustomWildSpawnType
    {
        private int wildSpawnTypeValue = 1000;
        private string wildSpawnTypeName = "example";
        private string scavRole = "Scav";
        private int baseBrain = 1; // Corresponds to EFT.WildSpawnType
        private bool isBoss = false; // Required to be true if bot is meant to have followers (raiders and rogues have both isBoss true and isFollower true)
        private bool isFollower = false; // Required to be true so bot is treated as a follower
        private bool isHostileToEverybody = false;
        private bool? countAsBossForStatistics = null; // If null, defaults to isBoss value

        public CustomWildSpawnType(int value, string name, string scavRole, int baseBrain, bool isBoss = false, bool isFollower = false, bool isHostileToEverybody = false)
        {
            this.wildSpawnTypeValue = value;
            this.wildSpawnTypeName = name;
            this.scavRole = scavRole;
            this.baseBrain = baseBrain;
            this.isBoss = isBoss;
            this.isFollower = isFollower;
            this.isHostileToEverybody = isHostileToEverybody;
        }

        public void SetCountAsBossForStatistics(bool? countAsBoss)
        {
            this.countAsBossForStatistics = countAsBoss;
        }

        public int WildSpawnTypeValue { get => wildSpawnTypeValue; }
        public string WildSpawnTypeName { get => wildSpawnTypeName; }
        public string ScavRole { get => scavRole; }
        public int BaseBrain { get => baseBrain; }
        public bool IsBoss { get => isBoss; }
        public bool IsFollower { get => isFollower; }
        public bool IsHostileToEverybody { get => isHostileToEverybody; }
        public bool? CountAsBossForStatistics { get => countAsBossForStatistics; }
    }
}
