using Magic.Models.Chat;
using Magic.Models.DataContext;
using Microsoft.AspNet.SignalR;
using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Penna.Messaging.Web;

namespace Magic
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Initialise database.
            System.Data.Entity.Database.SetInitializer<MagicDbContext>(null);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            SimpleInjectorConfig.RegisterContainer();

            // Add ChatLog and stealthy Cache scheduler.
            //this.Application["GeneralChatLog"] = new ChatLog();
            //RecurringTask("SaveChatLog", 3);

            // Enable automatic migrations.
            //var migrator = new System.Data.Entity.Migrations.DbMigrator(new Migrations.Configuration());
            //migrator.Update();

            // Initialise dependency injection resolver.
            //Magic.App_Start.SimpleInjectorInitializer.Initialize();

            // Clears WebFormsViewEngine (no longer searching for .aspx files).
            //ViewEngines.Engines.Clear();
            // Registers only Razor C# specific view engine.
            // This can also be registered using dependency injection through the new IDependencyResolver interface.
            //ViewEngines.Engines.Add(new RazorViewEngine());

            #region HUB CONFIG
            // Make long polling connections wait a maximum of 110 seconds for a
            // response. When that time expires, trigger a timeout command and
            // make the client reconnect.
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);

            // Wait a maximum of 30 seconds after a transport connection is lost
            // before raising the Disconnected event to terminate the SignalR connection.
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(6);

            // For transports other than long polling, send a keepalive packet every
            // 10 seconds. 
            // This value must be no more than 1/3 of the DisconnectTimeout value.
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(2);
            #endregion HUB CONFIG
        }

        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    Exception exception = Server.GetLastError();
        //    Response.Clear();

        //    var httpException = exception as HttpException;

        //    if (httpException != null)
        //    {
        //        string action;

        //        switch (httpException.GetHttpCode())
        //        {
        //            case 404:
        //                // page not found
        //                action = "NotFound";
        //                break;
        //            case 403:
        //                // forbidden
        //                action = "Forbidden";
        //                break;
        //            case 500:
        //                // server error
        //                action = "HttpError500";
        //                break;
        //            default:
        //                action = "Unknown";
        //                break;
        //        }

        //        // clear error on server
        //        Server.ClearError();

        //        Response.Redirect(String.Format("~/Errors/{0}", action));
        //    }
        //    else
        //    {
        //        // this is my modification, which handles any type of an exception.
        //        Response.Redirect(String.Format("~/Errors/Unknown"));
        //    }
        //}

        protected void Session_End(Object sender, EventArgs E)
        {
            // Remove from chat? after 20 mins...
        }

        #region CHATLOG SAVE
        // Schedule ChatLog saving with stealthy Cache object.
        private static CacheItemRemovedCallback OnCacheRemove = null;
        private void RecurringTask(string name, int minutes)
        {
            OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
            HttpRuntime.Cache.Insert(name, minutes, null,
                DateTime.Now.AddMinutes(minutes), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove);
        }

        public void CacheItemRemoved(string name, object value, CacheItemRemovedReason r)
        {
            // Clean and process ChatLog. 
            SaveChatLog();
            RecurringTask(name, Convert.ToInt32(value));
        }

        public bool SaveChatLog(bool save = true, bool skipInterval = true, TimeSpan? intervalMinutes = null)
        {
            var currentLog = (ChatLog) this.Application["GeneralChatLog"];
            if (currentLog != null && currentLog.Messages.Count > 10)
            {
                //if (!skipInterval)
                //{
                //    // Use default value for logging interval.
                //    if (intervalMinutes == null)
                //        intervalMinutes = new TimeSpan(0, 5, 0);

                //    TimeSpan currentLoggingTime = DateTime.Now - currentLog.DateCreated;
                //    if (currentLoggingTime < intervalMinutes)
                //    {
                //        // ChatLog saving interval has not passed yet, return.
                //        return false;
                //    }
                //}

                // Synchronize clearing the current ChatLog to save memory but preserve recent messages.
                this.Application.Lock();
                this.Application["GeneralChatLog"] = new ChatLog();
                ((ChatLog) this.Application["GeneralChatLog"]).AppendMessages(currentLog.Messages.GetRange(currentLog.Messages.Count - 10, 10));
                this.Application.UnLock();

                if (save)
                {
                    // Proceed with saving captured ChatLog to database.
                    //ChatHub.SaveChatLogToDatabase(currentLog);
                }
            }
            return true;
        }
        #endregion CHATLOG SAVE
    }
}