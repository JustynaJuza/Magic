using System.IO;
using System.Web.Mvc;

namespace Juza.Magic.Helpers
{
    public static class ControllerExtensions
    {
        public static string RenderRazorViewToString(this Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, writer);

                viewResult.View.Render(viewContext, writer);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}