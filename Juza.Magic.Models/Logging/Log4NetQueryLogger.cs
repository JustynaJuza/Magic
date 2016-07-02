using log4net;
using Newtonsoft.Json;
using Penna.Services.Contract.Infrastructure;
using System;

namespace Penna.Services.Core.Logging
{
    public class Log4NetQueryLogger<TQuery, TResult> : IQueryLogger<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        private readonly ISqlLogReader _sqlLogReader;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TQuery));

        public Log4NetQueryLogger(ISqlLogReader sqlLogReader)
        {
            _sqlLogReader = sqlLogReader;
        }

        public void Log(TQuery query, TResult result, TimeSpan duration)
        {
            Logger.Info(new QueryLoggingFormatter(query, result, duration));
            Logger.Debug(_sqlLogReader.Read());
        }

        public void LogError(TQuery query, TimeSpan duration, Exception exception)
        {
            Logger.Error(new QueryLoggingFormatter(query, duration), exception);
            Logger.Debug(_sqlLogReader.Read());
        }

        class QueryLoggingFormatter
        {
            private readonly TQuery _query;
            private readonly TResult _result;
            private readonly TimeSpan _duration;

            public QueryLoggingFormatter(TQuery query, TResult result, TimeSpan duration)
                : this(query, duration)
            {
                _result = result;
            }

            public QueryLoggingFormatter(TQuery query, TimeSpan duration)
            {
                _query = query;
                _duration = duration;
            }

            public override string ToString()
            {
                if (_result == null)
                {
                    return String.Format("Duration: {0}ms | Params: {1}",
                        _duration.TotalMilliseconds,
                        JsonConvert.SerializeObject(_query));   
                }
                else
                {
                    return String.Format("Duration: {0}ms | Params: {1} | Result: {2}",
                        _duration.TotalMilliseconds,
                        JsonConvert.SerializeObject(_query),
                        JsonConvert.SerializeObject(_result));   
                }
            }
        }
    }
}