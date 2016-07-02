using Penna.Services.Contract.Infrastructure;
using System;

namespace Penna.Services.Core.Logging
{
    public interface ICommandLogger<in TCommand>
        where TCommand : ICommand
    {
        void Log(TCommand command, TimeSpan duration);
        void LogError(TCommand command, TimeSpan duration, Exception exception);
    }
}