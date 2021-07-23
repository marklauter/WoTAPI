using System.Collections.Generic;
using Newtonsoft.Json;

namespace WoT.API.Sandbox.Models
{
    public class Reserve
    {
        public string type { get; set; }
        public int level { get; set; }
        public Account account { get; set; }
        public bool is_prolonged { get; set; }
    }
}
