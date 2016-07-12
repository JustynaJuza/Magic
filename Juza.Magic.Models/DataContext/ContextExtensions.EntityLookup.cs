using System;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Juza.Magic.Models.DataContext
{
    public static partial class ContextExtensions
    {
        public static IEntityLookup<TEntity> Read<TEntity>(this IDbContext context)
           where TEntity : class
        {
            var entityLookup = new EntityLookup<TEntity>(context.FindOrFetchEntity);
            return entityLookup;
        }

        private static TEntity FindOrFetchEntity<TEntity>(this IDbContext context, IEntityLookup<TEntity> entityLookup, params object[] keyValues)
           where TEntity : class
        {
            if (!entityLookup.SkipLocalContextCheck)
            {
                var localEntity = context.FindLocal<TEntity>(keyValues);
                if (localEntity != null)
                {
                    // get the entity's context entry
                    var contextEntry = context.Entry(localEntity);

                    // check if all includes are loaded and skip db call if everything required is already present
                    if (entityLookup.CollectionExpressions.All(
                        memberExpression => contextEntry.Collection(memberExpression).IsLoaded)
                        &&
                        entityLookup.ReferenceExpressions.All(
                            memberExpression => contextEntry.Reference(memberExpression).IsLoaded))
                    {
                        return localEntity;
                    }
                }
            }

            var collectionsQuery = entityLookup.CollectionExpressions.Aggregate(
                context.Set<TEntity>().AsQueryable(),
                (set, memberExpression) => set.Include(memberExpression));
            var referencesQuery = entityLookup.ReferenceExpressions.Aggregate(
                context.Set<TEntity>().AsQueryable(),
                (set, memberExpression) => set.Include(memberExpression));

            var keyPropertyNames = context.GetEntityKeyNames<TEntity>();

            // if entity is not present in context or includes are missing make a new db call with everything included
            return collectionsQuery.Concat(referencesQuery)
                .FirstOrDefault(EntityExtensions.MatchKey<TEntity>(keyPropertyNames, keyValues));
        }

        public static TEntity FindLocal<TEntity>(this IDbContext context, params object[] keyValues)
                where TEntity : class
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var keyPropertyNames = context.GetEntityKeyNames<TEntity>();
            var keyExpression = EntityExtensions.MatchKey<TEntity>(keyPropertyNames, keyValues).Compile();

            return context.Set<TEntity>().Local.FirstOrDefault(keyExpression);
        }

        private static string[] GetEntityKeyNames<TEntity>(this IDbContext context)
            where TEntity : class
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var objectContext = ((IObjectContextAdapter) context).ObjectContext;

            // we must use the namespace of the context and the type name of the entity
            var entityTypeName = context.GetType().Namespace + '.' + typeof(TEntity).Name;
            var entityType = objectContext.MetadataWorkspace.GetItem<EntityType>(entityTypeName, DataSpace.CSpace);
            return entityType.KeyProperties.Select(k => k.Name).ToArray();
        }

    }
}