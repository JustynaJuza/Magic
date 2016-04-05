using System.Collections.Generic;
using Newtonsoft.Json;

namespace Magic.Models.JQueryDataTables
{
    public class DataTablesRequestOut
    {
        [JsonProperty("draw")]
        public int Draw { get; set; }
        [JsonProperty("recordsTotal")]
        public int RecordsTotal { get; set; }
        [JsonProperty("recordsFiltered")]
        public int RecordsFiltered { get; set; }
        [JsonProperty("data")]
        public IList<string[]> Data { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}