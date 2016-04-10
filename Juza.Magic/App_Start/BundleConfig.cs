using System.Web.Optimization;

namespace Juza.Magic
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery-ui-slider-pips.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryajax").Include(
                        "~/Scripts/jquery.unobtrusive-ajax*",
                        "~/Scripts/_ajax-loading.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/jquerydatatables").Include(
                      "~/Scripts/jquery.datatables/jquery.dataTables*",
                      "~/Scripts/jquery.datatables/dataTables.bootstrap*",
                      "~/Scripts/jquery.datatables/dataTables.jqueryui*",
                      "~/Scripts/jquery.datatables/_datatables-init.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/jquerydatatables").IncludeDirectory(
                      "~/Content/jquery.datatables", "*.css"));

            bundles.Add(new StyleBundle("~/Content/jqueryui").IncludeDirectory(
                        "~/Content/jquery.ui", "*.css"));
        }
    }
}
