using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rbl.Models.Report
{
    public class BarScoreResponse
    {
        [JsonProperty("type")] 
        public string Type { get; set; }

        [JsonProperty("category")] 
        public string Category { get; set; }

        [JsonProperty("from")] 
        public decimal FromScore { get; set; }

        [JsonProperty("to")] 
        public decimal ToScore { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}
