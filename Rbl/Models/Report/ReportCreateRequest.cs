using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rbl.Models.Report
{
    public class ReportCreateRequest
    {
        #region Properties

        [Required, JsonProperty("ticker")]
        public string Ticker { get; set; }

        [Required, JsonProperty("name")]
        public string Name { get; set; }

 
        #endregion
    }
}

