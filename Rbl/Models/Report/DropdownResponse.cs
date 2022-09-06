using Newtonsoft.Json;

namespace Rbl.Models.Report
{
    public class DropdownResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}