using Juza.Magic.Models.Projections;
using System;
using System.Linq;
using System.Reflection;

namespace Juza.Magic.Models.Extensions
{
    public static class ObjectExtensions
    {
        //public static TViewModel ToViewModel<TModel, TViewModel>(this TModel model, params object[] args)
        //    where TModel : class
        //    where TViewModel : IViewModel<TModel>
        //{
        //    return args.Length > 0
        //        ? (TViewModel) Activator.CreateInstance(typeof(TViewModel), model, args)
        //        : (TViewModel) Activator.CreateInstance(typeof(TViewModel), model);
        //}

        public static TViewModel ToViewModel<TModel, TViewModel>(this TModel model)
        {
            var projectionType = typeof(IObjectMapping<TModel, TViewModel>);

            // Get mapping type defined in projection assembly (should be in Mappings directory)
            // If no type is defined use auto mapping.
            var projectionConfig = Assembly
                .GetAssembly(projectionType)
                .GetTypes()
                .FirstOrDefault(type => type.GetInterfaces().Any(i => i == projectionType))
                ?? typeof(DefaultAutoMapping<TModel, TViewModel>);

            var projection = (IObjectMapping<TModel, TViewModel>) Activator.CreateInstance(projectionConfig);

            return projection.Apply(model);
        }

        public static string ToContentString(this object model)
        {
            var modelType = model.GetType();
            var objName = modelType.FullName + ": ";
            var classMembers = modelType.GetProperties();

            return classMembers.Aggregate(objName, (toString, member) => toString + ("\n" + member.Name + " : " + member.GetValue(model) + "; "));
        }

        public static string ToHtmlString(this object model)
        {
            return ToContentString(model).Replace("\n", "<br />");
            //return WebUtility.HtmlEncode(ToContentString(model).Replace("\n", "<br />"));
        }
    }
}