using System;
using System.Web;
using Elmah;

namespace Magic.ErrorHandling
{
    public static class Error
    {
        public static void Log(Exception error)
        {
            if (error == null)
                return;
            if (HttpContext.Current == null) //In case we run outside of IIS
                ErrorLog.GetDefault(null).Log(new Elmah.Error(error));
            ErrorSignal.FromCurrentContext().Raise(error);
        }
    }
}