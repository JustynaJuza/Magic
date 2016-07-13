using Juza.Magic.Models.Interfaces;
using Juza.Magic.Models.Projections;
using System;
using System.Linq;
using System.Reflection;

namespace Juza.Magic.Models.Extensions
{
    /// <summary>
    /// Convenience extension methods for chaining
    /// </summary>
    public static class ProjectionExtensions
    {
        public static IQueryable<TViewModel> Project<TModel, TViewModel>(this IQueryable<TModel> source)
            where TModel : class
            where TViewModel : IViewModel<TModel>
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

            return projection.Apply(source);
        }
    }

}