using System.Collections.Generic;

namespace Juza.Magic.Models.JQueryDataTables
{
    public class DataTablesRequestOut
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public IList<string[]> Data { get; set; }
        public string Error { get; set; }
    }
}