using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rbl.Models.Report
{
    public class ReportResponse
    {
        [JsonProperty("ticker")]
        public string Ticker { get; set; }
        [Required, JsonProperty("industryCode")]
        public string IndustryCode { get; set; }
        [Required, JsonProperty("hasExtendedData")]
        public bool HasExtendedData { get; set; }

        [JsonProperty("scoresTopTen")]
        public GeneralScoreResponse ScoresTopTen { get; set; }

        [JsonProperty("scoresByIndustry")]
        public GeneralScoreResponse ScoresByIndustry { get; set; }

        [JsonProperty("scoresAll")]
        public GeneralScoreResponse ScoresAll { get; set; }

        [JsonProperty("scoresByTicker")]
        public GeneralScoreResponse ScoresByTicker { get; set; }
    }
}
