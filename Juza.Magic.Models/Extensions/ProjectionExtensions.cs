using Juza.Magic.Models.Interfaces;
using Juza.Magic.Models.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Juza.Magic.Models.Extensions
{
    /// <summary>
    /// Extension methods for projecting one type to another for queries, collections and objects
    /// </summary>
    public static class ProjectionExtensions
    {
        /// <summary>
        /// Map TModel object to TViewModel using mapping config defined in assembly or DefaultAutoMapping
        /// </summary>
        public static TViewModel ToViewModel<TModel, TViewModel>(this TModel model)
        {
            var projectionConfig = GetProjectionConfigForType<TModel, TViewModel>(typeof(IObjectMapping<TModel, TViewModel>));

            var projection = (IObjectMapping<TModel, TViewModel>) Activator.CreateInstance(projectionConfig);

            return projection.Apply(model);
        }

        /// <summary>
        /// Project entities in query from TModel to TViewModel using LINQ to SQL
        /// </summary>
        public static IQueryable<TViewModel> Project<TModel, TViewModel>(this IQueryable<TModel> source)
            where TModel : class
            where TViewModel : IViewModel<TModel>
        {
            var projectionConfig = GetProjectionConfigForType<TModel, TViewModel>(typeof(IQueryMapping<TModel, TViewModel>));

            var projection = (IQueryMapping<TModel, TViewModel>) Activator.CreateInstance(projectionConfig);

            return projection.Apply(source);
        }

        /// <summary>
        /// Project objects in collection from TModel to TViewModel
        /// </summary>
        public static IEnumerable<TViewModel> Project<TModel, TViewModel>(this IEnumerable<TModel> source)
            where TModel : class
            where TViewModel : IViewModel<TModel>
        {
            var projectionConfig = GetProjectionConfigForType<TModel, TViewModel>(typeof(IObjectMapping<TModel, TViewModel>));

            var projection = (IObjectMapping<TModel, TViewModel>) Activator.CreateInstance(projectionConfig);

            return projection.Apply(source);
        }

        /// <summary>
        /// Get mapping config defined for projection from TModel to TViewModel (should be in calling assembly).
        /// If no type is defined use auto mapping.
        /// </summary>
        private static Type GetProjectionConfigForType<TModel, TViewModel>(Type projectionType)
        {
            return Assembly
                .GetCallingAssembly()
                .GetTypes()
                .FirstOrDefault(type => type.GetInterfaces().Any(i => i == projectionType))
                ?? typeof(DefaultAutoMapping<TModel, TViewModel>);
        }
    }

}