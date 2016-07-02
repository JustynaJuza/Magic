using Penna.Services.Contract.Infrastructure;
using System;

namespace Penna.Services.Core.Logging
{
    public interface IQueryLogger<in TQuery, in TResult>
        where TQuery : IQuery<TResult>
    {
        void Log(TQuery query, TResult result, TimeSpan duration);
        void LogError(TQuery query, TimeSpan duration, Exception exception);
    }
}