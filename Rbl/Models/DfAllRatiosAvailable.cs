using System;
using System.Collections.Generic;

#nullable disable

namespace Rbl.Models
{
    public partial class DfAllRatiosAvailable
    {
        public int Id { get; set; }
        public string Ticker { get; set; }
        public double? Cik { get; set; }
        public string AccessionNumber { get; set; }
        public DateTime? FilingDate { get; set; }
        public string Text { get; set; }
        public string Mdna { get; set; }
        public string Embedding { get; set; }
        public string HcParagraphs { get; set; }
        public string HcSentences { get; set; }
        public double? Agility { get; set; }
        public double? Capability { get; set; }
        public double? Competence { get; set; }
        public double? Culture { get; set; }
        public double? Develop { get; set; }
        public double? Employee { get; set; }
        public double? Hc { get; set; }
        public double? Hr { get; set; }
        public double? Leadership { get; set; }
        public double? Management { get; set; }
        public double? Mission { get; set; }
        public double? Organization { get; set; }
        public double? Talent { get; set; }
        public double? Vision { get; set; }
        public double? Fraud { get; set; }
        public double? Litigious { get; set; }
        public double? BookValue { get; set; }
        public double? CurrentPrice { get; set; }
        public double? PriceToBook { get; set; }
        public double? EbitdaMargins { get; set; }
        public double? RevenueGrowth { get; set; }
        public double? RecommendationMean { get; set; }
        public string RecommendationKey { get; set; }
        public double? NumberOfAnalystOpinions { get; set; }
        public double? DebtToEquity { get; set; }
        public double? ReturnOnEquity { get; set; }
        public double? HeldPercentInstitutions { get; set; }
        public double? TrailingEps { get; set; }
        public double? Beta { get; set; }
        public double? PegRatio { get; set; }
        public double? ForwardPe { get; set; }
        public double? TotalRevenue { get; set; }
        public double? FullTimeEmployees { get; set; }
        public double? MarketCap { get; set; }
        public double? RevenuePerEmployee { get; set; }
    }
}
