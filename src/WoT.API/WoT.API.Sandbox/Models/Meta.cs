using System.Collections.Generic;
using Newtonsoft.Json;

namespace WoT.API.Sandbox.Models
{
    public class Meta
    {
        public int limit { get; set; }
        public int offset { get; set; }
        public List<AvailableBattleMode> available_battle_modes { get; set; }
        public int total_count { get; set; }
    }
}
