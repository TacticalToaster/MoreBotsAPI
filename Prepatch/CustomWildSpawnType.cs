
using System.Collections.Generic;

namespace MoreBotsAPI
{
    public class CustomWildSpawnType
    {
        // Enum int value of your bot
        private int _wildSpawnTypeValue = 1000;
        // Enum name for your bot
        private string _wildSpawnTypeName = "example";
        // Role used in the end of raid screen. Valid examples include Boss, Follower, Scav. You can add custom ones as long as you also add to locales
        private string _scavRole = "Scav";
        // Corresponds to EFT.WildSpawnType for brain of bots
        private int _baseBrain = 1;
        // Required to be true if bot is meant to have followers (raiders and rogues have both isBoss true and isFollower true)
        private bool _isBoss = false;
        // Required to be true so bot is treated as a follower
        private bool _isFollower = false;
        // This is set true for cultists
        private bool _isHostileToEverybody = false;
        // If null, defaults to isBoss value
        private bool? _countAsBossForStatistics = null;

        // If you have max fence rep and this is true, then the bot will not attempt to warn you. Doesn't affect hostility, that is defined in the json data.
        private bool _shouldUseFenceNoBossAttackScav = true;
        // Same as above but also for your pmc, in case you wanted to make your bot neutral to pmcs and warn them unless you have max fence rep
        private bool _shouldUseFenceNoBossAttackPMC = false;

        // Settings related to SAIN compatibility
        private SAINSettings _SAINSettings;

        // List of difficulties (EFT.BotDifficulty) that this type should NOT use
        private List<int> _excludedDifficulties;

        public CustomWildSpawnType(int value, string name, string scavRole, int baseBrain, bool isBoss = false, bool isFollower = false, bool isHostileToEverybody = false)
        {
            this._wildSpawnTypeValue = value;
            this._wildSpawnTypeName = name;
            this._scavRole = scavRole;
            this._baseBrain = baseBrain;
            this._isBoss = isBoss;
            this._isFollower = isFollower;
            this._isHostileToEverybody = isHostileToEverybody;
        }

        public void SetCountAsBossForStatistics(bool? countAsBoss)
        {
            this._countAsBossForStatistics = countAsBoss;
        }

        public void SetExcludedDifficulties(List<int> difficulties)
        {
            this._excludedDifficulties = difficulties;
        }

        public void SetShouldUseFenceNoBossAttack(bool shouldUseForScav, bool shouldUseForPMC = false)
        {
            this._shouldUseFenceNoBossAttackScav = shouldUseForScav;
            this._shouldUseFenceNoBossAttackPMC = shouldUseForPMC;
        }

        public void SetSAINSettings(SAINSettings settings)
        {
            this._SAINSettings = settings;
        }

        public int WildSpawnTypeValue { get => _wildSpawnTypeValue; }
        public string WildSpawnTypeName { get => _wildSpawnTypeName; }
        public string ScavRole { get => _scavRole; }
        public int BaseBrain { get => _baseBrain; }
        public bool IsBoss { get => _isBoss; }
        public bool IsFollower { get => _isFollower; }
        public bool IsHostileToEverybody { get => _isHostileToEverybody; }
        public bool? CountAsBossForStatistics { get => _countAsBossForStatistics; }
        public List<int> ExcludedDifficulties { get => _excludedDifficulties; }
        public bool ShouldUseFenceNoBossAttackScav { get => _shouldUseFenceNoBossAttackScav; }
        public bool ShouldUseFenceNoBossAttackPMC { get => _shouldUseFenceNoBossAttackPMC; }
        public SAINSettings SAINSettings { get => _SAINSettings; }
    }
}
