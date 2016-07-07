using Juza.Magic.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Juza.Magic.Models.DataContext
{
    public static partial class ContextExtensions
    {
        public static TEntity Read<TEntity>(this IDbContext context, params object[] keyValues)
            where TEntity : class
        {
            return context.Set<TEntity>().Find(keyValues);
        }

        public static IEntityLookup<TEntity> Read<TEntity>(this IDbContext context)
           where TEntity : class
        {
            var entityLookup = new EntityLookup<TEntity>(context.FindOrFetchEntity);
            return entityLookup;
        }

        private static TEntity FindOrFetchEntity<TEntity>(this IDbContext context, IEntityLookup<TEntity> entityLookup, params object[] keyValue)
           where TEntity : class
        {
            // entity is already loaded in the context
            if (context.Exists<TEntity>(keyValue))
            {
                // get the entity's context entry
                var localEntity = context.Set<TEntity>().Find(keyValue);
                var contextEntry = context.Entry(localEntity);

                // check if all includes are loaded and skip db call if everything required is already present
                if (entityLookup.CollectionExpressions.All(memberExpression => contextEntry.Collection(memberExpression).IsLoaded)
                    && entityLookup.ReferenceExpressions.All(memberExpression => contextEntry.Reference(memberExpression).IsLoaded))
                {
                    return localEntity;
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
                .FirstOrDefault(EntityExtensions.MatchKey<TEntity>(keyPropertyNames, keyValue));
        }

        //public static TEntity ReadWithIncluded<TEntity>(this IDbContext context, int id, params Expression<Func<TEntity, object>>[] includes)
        //    where TEntity : class
        //{
        //    return includes.Aggregate(context.Set<TEntity>().AsQueryable(), (set, include) => set.Include(include))
        //        .GetById(id)
        //        .SingleOrDefault();
        //}

        public static TEntity GetOrCreate<TEntity>(this IDbContext context, int? id)
            where TEntity : class, new()
        {
            if (id.HasValue)
                return Read<TEntity>(context, id.Value);

            var cc = new TEntity();
            Insert(context, cc);
            return cc;
        }

        public static TEntity GetOrCreate<TEntity>(this IDbContext context, int? id, IEnumerable<Expression<Func<TEntity, object>>> includes)
            where TEntity : class, new()
        {
            return GetOrCreate(context, id, includes.ToArray());
        }

        //public static TEntity GetOrCreate<TEntity>(this IDbContext context, int? id, params Expression<Func<TEntity, object>>[] includes)
        //    where TEntity : class, new()
        //{
        //    if (id.HasValue && id != 0)
        //        return ReadWithIncluded(context, id.Value, includes);

        //    var cc = new TEntity();
        //    Insert(context, cc);
        //    return cc;
        //}

        private static TEntity ReadById<TEntity>(this IDbContext context, TEntity entity)
            where TEntity : class
        {
            var collectionType = entity.GetType();
            var itemKeyInfo = collectionType.GetProperty("Id") ?? collectionType.GetProperty(collectionType.Name + "Id");

            if (itemKeyInfo == null)
            {
                var entityException = new TargetException("The entity could not be read, " +
                                                          "no valid Id was found with the name 'Id' or '" +
                                                          collectionType.Name + "Id', " +
                                                          "please use the Read method overload which accepts a key structure");
                entityException.LogException();
                throw entityException;
            }

            var itemKey = itemKeyInfo.GetValue(entity);
            var itemType = itemKeyInfo.PropertyType;

            // if the key is at the default value we can assume a new entity already exists in context which is attached but not saved
            // == will not return true because that's the way boxed int values compare, they could be cast as (dynamic) to compare with ==
            if (itemKey.Equals(itemType.GetDefaultValue()))
                return null;

            var foundEntity = context.Set<TEntity>().Find(itemKey);
            return foundEntity;
        }

        public static void Insert<TEntity>(this IDbContext context, TEntity entity)
            where TEntity : class
        {
            context.Entry(entity).State = EntityState.Added;
        }

        public static bool InsertAndSave<TEntity>(this IDbContext context, TEntity entity)
            where TEntity : class
        {
            Insert(context, entity);
            return context.SaveChanges() > 0;
        }

        public static void Update<TEntity>(this IDbContext context, TEntity existingEntity, TEntity newEntity)
            where TEntity : class
        {
            context.Entry(existingEntity).CurrentValues.SetValues(newEntity);
        }

        public static bool UpdateAndSave<TEntity>(this IDbContext context, TEntity existingEntity, TEntity newEntity)
            where TEntity : class
        {
            Update(context, existingEntity, newEntity);
            return context.SaveChanges() > 0;
        }

        private static void InsertOrUpdate<TEntity>(this IDbContext context, TEntity existingEntity, TEntity newEntity)
            where TEntity : class
        {
            if (existingEntity == null)
            {
                Insert(context, newEntity);
            }
            else
            {
                Update(context, existingEntity, newEntity);
            }
        }

        public static void InsertOrUpdate<TEntity>(this IDbContext context, TEntity entity)
            where TEntity : class
        {
            var existingEntity = ReadById(context, entity);
            InsertOrUpdate(context, existingEntity, entity);
        }

        public static void InsertOrUpdate<TEntity>(this IDbContext context, TEntity entity, object[] keyValues)
            where TEntity : class
        {
            var existingEntity = context.Set<TEntity>().Find(keyValues);
            InsertOrUpdate(context, existingEntity, entity);
        }

        public static bool InsertOrUpdateAndSave<TEntity>(this IDbContext context, TEntity entity)
            where TEntity : class
        {
            InsertOrUpdate(context, entity);
            return context.SaveChanges() > 0;
        }

        public static bool InsertOrUpdateAndSave<TEntity>(this IDbContext context, TEntity entity, object[] keyValues)
            where TEntity : class
        {
            var existingEntity = context.Set<TEntity>().Find(keyValues);
            InsertOrUpdate(context, existingEntity, entity);
            return context.SaveChanges() > 0;
        }

        public static void Delete<TEntity>(this IDbContext context, TEntity entity)
            where TEntity : class
        {
            context.Entry(entity).State = EntityState.Deleted;
        }

        public static bool DeleteAndSave<TEntity>(this IDbContext context, TEntity entity)
            where TEntity : class
        {
            Delete(context, entity);
            return context.SaveChanges() > 0;
        }

        public static void FindAndDelete<TEntity, TKey>(this IDbContext context, TKey key)
            where TEntity : class
        {
            var existingEntity = context.Set<TEntity>().Find(key);
            Delete(context, existingEntity);
        }
        public static void FindAndDeleteAndSave<TEntity, TKey>(this IDbContext context, TKey key)
            where TEntity : class
        {
            var existingEntity = context.Set<TEntity>().Find(key);
            DeleteAndSave(context, existingEntity);
        }

        public static void FindAndDelete<TEntity>(this IDbContext context, object[] keyValues)
            where TEntity : class
        {
            var existingEntity = context.Set<TEntity>().Find(keyValues);
            Delete(context, existingEntity);
        }

        public static void FindAndDeleteAndSave<TEntity>(this IDbContext context, object[] keyValues)
            where TEntity : class
        {
            var existingEntity = context.Set<TEntity>().Find(keyValues);
            DeleteAndSave(context, existingEntity);
        }
    }
}