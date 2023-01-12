using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rbl.Models.Report
{
    public class GeneralScoreResponse
    {
        [Required, JsonProperty("talentScore")]
        public double TalentScore { get; set; }
        [Required, JsonProperty("leadershipScore")]
        public double LeadershipScore { get; set; }
        [Required, JsonProperty("hrScore")]
        public double HrScore { get; set; }
        [Required, JsonProperty("orgScore")]
        public double OrganizationScore { get; set; }
        public double OrgScore => OrganizationScore;
        [Required, JsonProperty("overallScore")]
        public double OverallScore { get; set; }
        public double TotalScore { get; set; }
    }
}
