using System;
using System.Linq;
using System.Web;
using Elmah;

namespace Juza.Magic.Models.Extensions
{
    public static class ExceptionExtensions
    {
        public static void LogException(this Exception exception)
        {
            if (HttpContext.Current == null)
            {
                //In case we run outside of IIS
                ErrorLog.GetDefault(null).Log(new Error(exception));
            }
            else
            {
                ErrorSignal.FromCurrentContext().Raise(exception);
            }
        }

        public static bool HandleException(this Exception exception, params Type[] exceptionTypes)
        {
            if (exceptionTypes == null)
            {
                return false;
            }

            var exceptionType = exception.GetType();
            return exceptionTypes.Any(ex => ex.IsAssignableFrom(exceptionType));
        }
    }
}