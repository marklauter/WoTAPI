using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace WoT.API.Sandbox.Models
{
    public class AdvancesPage
    {
        public Meta meta { get; set; }
        public Data[] data { get; set; }
    }

    public class Data
    {
        public int series_id { get; set; }
        public int id { get; set; }
        public string type { get; set; }
        public int industrial_resource { get; set; }
        public int winner_clan_id { get; set; }
        public string direction { get; set; }
        public Rounds[] rounds { get; set; }
        public EnemyClan enemy_clan { get; set; }
        public int captured_items_count { get; set; }
        public string finish_reason { get; set; }
        public Reserve[] reserves { get; set; }
        public int finished_at { get; set; }
        public string result { get; set; }
    }

    public class Rounds
    {
        public int[] frags { get; set; }
        public int series_id { get; set; }
        public int id { get; set; }
        public int max_vehicle_level { get; set; }
        public int respawn { get; set; }
        public string finish_reason { get; set; }
        public int industrial_resource { get; set; }
        public string type { get; set; }
        public int round { get; set; }
        public EnemyClan enemy_clan { get; set; }
        public int winner_clan_id { get; set; }
        public Arena arena { get; set; }
        public Reserve[] reserves { get; set; }
        public Commander commander { get; set; }
        public int finished_at { get; set; }
        public string result { get; set; }
        public double duration { get; set; }
    }

    
}
