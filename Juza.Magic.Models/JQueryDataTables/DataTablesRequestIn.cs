using System.Collections.Generic;
using System.Linq;

namespace Juza.Magic.Models.JQueryDataTables
{
    public class DataTablesRequestIn
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public DataTablesRequestSearch Search { get; set; }
        public IList<DataTablesRequestColumn> Columns { get; set; }
        public IList<DataTablesRequestOrder> Order { get; set; }

        public string SelectedColumns()
        {
            return Columns.Aggregate(Columns.ElementAt(0).Name, (s, c) => s + ',' + c.Name);
        }
    }

    public class DataTablesRequestSearch
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class DataTablesRequestOrder
    {
        public int Column { get; set; }
        public DataTablesRequestOrderDir Dir { get; set; }
    }

    public class DataTablesRequestColumn
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public DataTablesRequestSearch Search { get; set; }
    }

    public enum DataTablesRequestOrderDir
    {
        Asc,
        Desc
    }
}