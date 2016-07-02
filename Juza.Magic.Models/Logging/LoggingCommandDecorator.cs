using Penna.Services.Contract.Infrastructure;
using System;
using System.Diagnostics;

namespace Penna.Services.Core.Logging
{
    public class LoggingCommandDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _decorated;
        private readonly ICommandLogger<TCommand> _logger;

        public LoggingCommandDecorator(ICommandHandler<TCommand> decorated, ICommandLogger<TCommand> logger)
        {
            _decorated = decorated;
            _logger = logger;
        }

        public void Handle(TCommand command)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                _decorated.Handle(command);
                _logger.Log(command, timer.Elapsed);
            }
            catch (Exception e)
            {
                _logger.LogError(command, timer.Elapsed, e);
                throw;
            }
        }
    }
}
