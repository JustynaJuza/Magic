using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Juza.Magic.Helpers
{
    public interface IViewRenderer
    {
        /// <summary>
        /// Renders a full MVC view to a string. Will render with the full MVC
        /// View engine including running _ViewStart and merging into _Layout        
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to render the view with</param>
        /// <returns>String of the rendered view or null on error</returns>
        string RenderViewToString(string viewPath, object model = null);

        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <returns>String of the rendered view or null on error</returns>
        string RenderPartialViewToString(string viewPath, object model = null);
    }

    public class ViewRenderer : IViewRenderer
    {
        private static string RenderViewToString(ControllerContext context, string viewPath, object model = null)
        {
            var viewResult = ViewEngines.Engines.FindPartialView(context, viewPath)
                             ?? ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewResult == null)
            {
                throw new FileNotFoundException(string.Format("Cannot find view: {0}.", viewPath));
            }

            var view = viewResult.View;
            context.Controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(context, view, context.Controller.ViewData,
                    context.Controller.TempData, writer);

                viewResult.View.Render(viewContext, writer);
                viewResult.ViewEngine.ReleaseView(context, viewResult.View);

                return writer.ToString();
            }
        }

        public static T CreateController<T>(RouteData routeData = null)
            where T : Controller, new()
        {
            T controller = new T();

            HttpContextBase contextWrapper;
            if (HttpContext.Current != null)
            {
                contextWrapper = new HttpContextWrapper(HttpContext.Current);
            }
            else
            {
                throw new InvalidOperationException(
                    "Can't create Controller Context if no active HttpContext instance is available.");
            }

            if (routeData == null)
            {
                routeData = new RouteData();
            }

            // add the controller routing if not existing
            if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
            {
                routeData.Values.Add(
                    "controller",
                    controller.GetType().Name.ToLower().Replace("controller", ""));
            }

            controller.ControllerContext = new ControllerContext(contextWrapper, routeData, controller);
            return controller;
        }

        //public class ErrorModule : ApplicationErrorModule
        //{

        //    protected override void OnDisplayError(
        //                               WebErrorHandler errorHandler,
        //                               ErrorViewModel model)
        //    {
        //        var response = HttpContext.Current.Response;

        //        // Create an arbitrary controller instance
        //        var controller =
        //            ViewRenderer.CreateController<GenericController>();

        //        string html = ViewRenderer.RenderPartialView(
        //                                    "~/views/shared/Error.cshtml",
        //                                    model,
        //                                    controller.ControllerContext);

        //        HttpContext.Current.Server.ClearError();
        //        response.TrySkipIisCustomErrors = true;
        //        response.ClearContent();

        //        response.StatusCode = 500;
        //        response.Write(html);
        //    }
        //}

        // *any* controller class will do for the template
        public class GenericController : Controller
        {
        }


        /// <summary>
        /// Required Controller Context
        /// </summary>
        protected ControllerContext Context { get; set; }

        /// <summary>
        /// Initializes the ViewRenderer with a Context.
        /// </summary>
        /// <param name="controllerContext">
        /// If you are running within the context of an ASP.NET MVC request pass in
        /// the controller's context. 
        /// Only leave out the context if no context is otherwise available.
        /// </param>
        public ViewRenderer(/*ControllerContext controllerContext = null*/)
        {
            ControllerContext controllerContext = null;
            // Create a known controller from HttpContext if no context is passed
            if (controllerContext == null)
            {
                if (HttpContext.Current != null)
                    controllerContext = CreateController<EmptyController>().ControllerContext;
                else
                    throw new InvalidOperationException(
                        "ViewRenderer must run in the context of an ASP.NET " +
                        "Application and requires HttpContext.Current to be present.");
            }
            Context = controllerContext;
        }

        /// <summary>
        /// Renders a full MVC view to a string. Will render with the full MVC
        /// View engine including running _ViewStart and merging into _Layout        
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to render the view with</param>
        /// <returns>String of the rendered view or null on error</returns>
        public string RenderViewToString(string viewPath, object model = null)
        {
            return RenderViewToStringInternal(viewPath, model, false);
        }


        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <returns>String of the rendered view or null on error</returns>
        public string RenderPartialViewToString(string viewPath, object model = null)
        {
            return RenderViewToStringInternal(viewPath, model, true);
        }

        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <param name="controllerContext">Active Controller context</param>
        /// <returns>String of the rendered view or null on error</returns>
        //public static string RenderView(string viewPath, object model = null,
        //    ControllerContext controllerContext = null)
        //{
        //    ViewRenderer renderer = new ViewRenderer(controllerContext);
        //    return renderer.RenderViewToString(viewPath, model);
        //}



        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <param name="controllerContext">Active controller context</param>
        /// <returns>String of the rendered view or null on error</returns>
        //public static string RenderPartialView(string viewPath, object model = null,
        //    ControllerContext controllerContext = null)
        //{
        //    ViewRenderer renderer = new ViewRenderer(controllerContext);
        //    return renderer.RenderPartialViewToString(viewPath, model);
        //}


        /// <summary>
        /// Internal method that handles rendering of either partial or 
        /// or full views.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">Model to render the view with</param>
        /// <param name="partial">Determines whether to render a full or partial view</param>
        /// <returns>String of the rendered view</returns>
        private string RenderViewToStringInternal(string viewPath, object model,
            bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(Context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(Context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("Requested view was not found.");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            Context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(Context, view,
                    Context.Controller.ViewData,
                    Context.Controller.TempData,
                    sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }

        /// <summary>
        /// Empty MVC Controller instance used to 
        /// instantiate and provide a new ControllerContext
        /// for the ViewRenderer
        /// </summary>
        public class EmptyController : Controller
        {
        }
    }
}