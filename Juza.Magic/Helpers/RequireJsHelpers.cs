using System;
using System.Web.Mvc;

namespace Juza.Magic.Helpers
{
    public static class RequireJsHelpers
    {
        private static readonly string scriptsPath = "/Scripts/";
        private static readonly string requireConfigFile = "requireConfig.js";

        public static MvcHtmlString RequireScriptModule(this HtmlHelper helper, params string[] modules)
        {
            if (modules.Length == 0)
            {
                throw new ArgumentException("No parameters passed. This function should be called with at least one parameter.");
            }

            var requireScript = string.Format(
                @"require(['{0}{1}'], function() {{
                      require(['{2}']);
                  }});",
                scriptsPath,
                requireConfigFile,
                string.Join("','", modules));

            return new MvcHtmlString(requireScript);
        }

        public static MvcHtmlString RequireConditionalScriptModule(this HtmlHelper helper, string module, string dependency)
        {
            var requireScript = string.Format(
                @"if (require.specified('{2}')) {{
                      require(['{0}{1}'], function() {{
                          require(['{2}'], function () {{
                              require(['{3}']);
                          }});
                      }});
                  }}",
                scriptsPath,
                requireConfigFile,
                dependency,
                module);

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