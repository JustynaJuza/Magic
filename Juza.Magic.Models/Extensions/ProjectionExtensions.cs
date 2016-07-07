using Juza.Magic.Models.Interfaces;
using Juza.Magic.Models.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Juza.Magic.Models.Extensions
{
    /// <summary>
    /// Convenience extension methods for chaining
    /// </summary>
    public static class ProjectionExtensions
    {
        public static IEnumerable<TDest> Project<TSource, TDest>(this IQueryable<TSource> source, IMapping<TSource, TDest> mapping)
        {
            return mapping.Apply(source);
        }

        public static IQueryable<TDest> Project<TSource, TDest>(this IQueryable<TSource> source, IQueryMapping<TSource, TDest> mapping)
        {
            return mapping.ApplyAsQuery(source);
        }

        public static IQueryable<TViewModel> ToViewModel<TModel, TViewModel>(this IQueryable<TModel> source)
            where TModel : class
            where TViewModel : IViewModel<TModel>
        {
            var projectionType = typeof(IQueryMapping<TModel, TViewModel>);

            // Get mapping type defined in projection assembly (should be in Mappings directory)
            // If no type is defined use auto mapping.
            var projectionConfig = Assembly
                .GetAssembly(projectionType)
                .GetTypes()
                .FirstOrDefault(type => type.GetInterfaces().Any(i => i == projectionType))
                ?? typeof(DefaultAutoMapping<TModel, TViewModel>);

            var projection = (IQueryMapping<TModel, TViewModel>) Activator.CreateInstance(projectionConfig);

            return projection.ApplyAsQuery(source);
        }
    }

}