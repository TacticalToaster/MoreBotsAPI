
using System.Collections.Generic;

namespace MoreBotsAPI
{
    public class CustomWildSpawnType
    {
        private int _wildSpawnTypeValue = 1000;
        private string _wildSpawnTypeName = "example";
        private string _scavRole = "Scav";
        private int _baseBrain = 1; // Corresponds to EFT.WildSpawnType
        private bool _isBoss = false; // Required to be true if bot is meant to have followers (raiders and rogues have both isBoss true and isFollower true)
        private bool _isFollower = false; // Required to be true so bot is treated as a follower
        private bool _isHostileToEverybody = false;
        private bool? _countAsBossForStatistics = null; // If null, defaults to isBoss value

        private bool _shouldUseFenceNoBossAttackScav = true;
        private bool _shouldUseFenceNoBossAttackPMC = false;

        private SAINSettings _SAINSettings;

        private List<int> _excludedDifficulties; // List of difficulties (EFT.BotDifficulty) that this type should NOT use

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
