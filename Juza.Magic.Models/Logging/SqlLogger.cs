
using System.Text;

namespace Penna.Services.Core.Logging
{
    public interface ISqlLogWriter
    {
        void Log(string sql);
    }

    public interface ISqlLogReader
    {
        string Read();
    }

    public class SqlLogger : ISqlLogWriter, ISqlLogReader
    {
        private readonly StringBuilder _sql = new StringBuilder();
 
        public void Log(string sql)
        {
            _sql.Append(sql);
        }

        public string Read()
        {
            return _sql.ToString();
        }
    }
}
