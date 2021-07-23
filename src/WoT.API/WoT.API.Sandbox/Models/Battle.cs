using System.Collections.Generic;
using Newtonsoft.Json;

namespace WoT.API.Sandbox.Models
{

    public class Battle
    {
        public Commander commander { get; set; }
        public Arena arena { get; set; }
        public double? duration { get; set; }
        public int industrial_resource { get; set; }
        public int id { get; set; }
        public string type { get; set; }
        public EnemyClan enemy_clan { get; set; }
        public string result { get; set; }
        public object series_id { get; set; }
        public int respawn { get; set; }
        public object round { get; set; }
        public List<Reserve> reserves { get; set; }
        public int max_vehicle_level { get; set; }
        public List<int> frags { get; set; }
        public string finish_reason { get; set; }
        public int finished_at { get; set; }
        public int? winner_clan_id { get; set; }
    }
}
