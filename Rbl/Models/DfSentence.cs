using System;
using System.Collections.Generic;

#nullable disable

namespace Rbl.Models
{
    public partial class DfSentence
    {
        public int Id { get; set; }
        public string Ticker { get; set; }
        public string TalentSentences { get; set; }
        public string LeadershipSentences { get; set; }
        public string OrganizationSentences { get; set; }
        public string HrSentences { get; set; }
    }
}
