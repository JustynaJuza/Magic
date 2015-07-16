using System;
using System.Linq;
using System.Web;
using Elmah;

namespace Magic.Helpers
{
    public static class ErrorHandler
    {
        public static void Log(Exception error)
        {
            if (error == null)
                return;
            if (HttpContext.Current == null) //In case we run outside of IIS
                ErrorLog.GetDefault(null).Log(new Elmah.Error(error));
            ErrorSignal.FromCurrentContext().Raise(error);
        }

        public static bool HandleException(this Exception exception, params Type[] exceptionTypes)
        {
            Log(exception);

            if (exceptionTypes == null)
            {
                return false;
            }

            var exceptionType = exception.GetType();
            return exceptionTypes.Contains(exceptionType);
        }
    }
}