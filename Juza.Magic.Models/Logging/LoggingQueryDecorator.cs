using Penna.Services.Contract.Infrastructure;
using System;
using System.Diagnostics;

namespace Penna.Services.Core.Logging
{
    public class LoggingQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> _decorated;
        private readonly IQueryLogger<TQuery, TResult> _logger;

        public LoggingQueryDecorator(IQueryHandler<TQuery, TResult> decorated, IQueryLogger<TQuery, TResult> logger)
        {
            _decorated = decorated;
            _logger = logger;
        }

        public TResult Handle(TQuery query)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                var result = _decorated.Handle(query);
                _logger.Log(query, result, timer.Elapsed);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(query, timer.Elapsed, e);
                throw;
            }
        }
    }
}
