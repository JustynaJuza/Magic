using System.Collections.Generic;

namespace Magic.Models.JQueryDataTables
{
    public class DataTablesRequestOut
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public IList<string[]> data { get; set; }
        public string error { get; set; }
    }
}