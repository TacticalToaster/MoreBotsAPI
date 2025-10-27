using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreBotsAPI
{
    public class SAINSettings
    {
        public int WildSpawnType;
        public string Name = "Default";
        public string Description = "Default Description";
        public string Section = "Default Section";
        public string BaseBrain = "DefaultBrain";
        public List<string> BrainsToApply;
        public List<string> LayersToRemove;

        public SAINSettings(int wildSpawnType)
        {
            WildSpawnType = wildSpawnType;
        }
    }
}
