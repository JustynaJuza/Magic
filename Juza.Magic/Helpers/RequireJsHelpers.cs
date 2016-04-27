using System.IO;
using System.Web.Mvc;

namespace Juza.Magic.Helpers
{
    public static class RequireJsHelpers
    {
        public static MvcHtmlString RequireScriptModule(this HtmlHelper helper, string module)
        {
            var scriptsPath = "/Scripts/";
            var requireConfigFile = "requireConfig.js";

            var isValidModule = File.Exists(
                helper.ViewContext.HttpContext.Server.MapPath(Path.Combine(scriptsPath, module + ".js")));

            var requireScript = isValidModule
                    ? string.Format(@"require([""{0}{1}"" ], function() {{
                                        require([ ""{2}"" ]);
                                    }});",
                                    scriptsPath,
                                    requireConfigFile,
                                    module)
                    : string.Empty;

            return new MvcHtmlString(requireScript);
        }

        public static MvcHtmlString RequireScriptsForCurrentRoute(this HtmlHelper helper)
        {
            var action = helper.ViewContext.RouteData.Values["action"];
            var controller = helper.ViewContext.RouteData.Values["controller"];

            return helper.RequireScriptModule(string.Format("views/{0}/{1}", controller, action));
        }
    }
}