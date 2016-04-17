using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Juza.Magic.Models.DataContext
{
    public static partial class ContextExtensions
    {
        public static bool Exists<TEntity>(this IDbContext context, TEntity entity)
            where TEntity : class
        {
            if (context == null)
                throw new ArgumentNullException("context");

            return context.Set<TEntity>().Local.Any(e => e == entity);
        }
        public static bool Exists<TEntity>(this IDbContext context, params object[] keyValue)
            where TEntity : class
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var keyPropertyNames = context.GetEntityKeyNames<TEntity>();

            return context.Set<TEntity>().Local.Any(EntityExtensions.MatchKey<TEntity>(keyPropertyNames, keyValue).Compile());
        }

        public static string[] GetEntityKeyNames<TEntity>(this IDbContext context)
            where TEntity : class
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var objectContext = ((IObjectContextAdapter)context).ObjectContext;

            // we must use the namespace of the context and the type name of the entity
            var entityTypeName = context.GetType().Namespace + '.' + typeof(TEntity).Name;
            var entityType = objectContext.MetadataWorkspace.GetItem<EntityType>(entityTypeName, DataSpace.CSpace);
            return entityType.KeyProperties.Select(k => k.Name).ToArray();
        }
    }
}