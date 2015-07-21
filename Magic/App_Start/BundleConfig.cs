using System.Web;
using System.Web.Optimization;

namespace Magic
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/jqueryajax").Include(
                        "~/Scripts/jquery.unobtrusive-ajax*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquerycolor").Include(
                        "~/Scripts/jquery.color-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquerysignalR").Include(
                        "~/Scripts/jquery.signalR-{version}.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/farbtastic").Include(
                        "~/Scripts/farbtastic.js",
                        "~/Scripts/_form-color-picker.js"));

            bundles.Add(new ScriptBundle("~/bundles/underscore").Include(
                        "~/Scripts/underscore.js",
                        "~/Scripts/underscore.string.js"));

            bundles.Add(new ScriptBundle("~/bundles/twitterBootstrap").Include(
                        "~/Scripts/twitter*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                      "~/Scripts/jquery.dataTables*",
                      "~/Scripts/dataTables*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/jquery-ui-{version}.css",
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/twitterBootstrap").Include(
                      "~/Content/twitter-bootstrap-css"));
            
            bundles.Add(new StyleBundle("~/Content/datatables").IncludeDirectory(
                      "~/Content/DataTables/css", "*.css"));
            
            bundles.Add(new StyleBundle("~/Content/farbtastic").Include(
                      "~/Content/farbtastic.css"));
        }
    }
}
