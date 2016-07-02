using log4net;
using Newtonsoft.Json;
using Penna.Services.Contract.Infrastructure;
using System;

namespace Penna.Services.Core.Logging
{
    public class Log4NetCommandLogger<TCommand> : ICommandLogger<TCommand> where TCommand : ICommand
    {
        private readonly ISqlLogReader _sqlLogReader;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TCommand));

        public Log4NetCommandLogger(ISqlLogReader sqlLogReader)
        {
            _sqlLogReader = sqlLogReader;
        }

        public void Log(TCommand command, TimeSpan duration)
        {
            Logger.Info(new CommandLoggingFormatter(command, duration));
            Logger.Debug(_sqlLogReader.Read());
        }

        public void LogError(TCommand command, TimeSpan duration, Exception exception)
        {
            Logger.Error(new CommandLoggingFormatter(command, duration), exception);
            Logger.Debug(_sqlLogReader.Read());
        }

        class CommandLoggingFormatter
        {
            private readonly TCommand _command;
            private readonly TimeSpan _duration;

            public CommandLoggingFormatter(TCommand command, TimeSpan duration)
            {
                _command = command;
                _duration = duration;
            }

            public override string ToString()
            {
                return String.Format("Duration: {0}ms | Params: {1}",
                    _duration.TotalMilliseconds,
                    JsonConvert.SerializeObject(_command));
            }
        }
    }
}
