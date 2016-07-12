using System.Data.Entity;

namespace Juza.Magic.Models.DataContext
{
    public static partial class ContextExtensions
    {
        public static TEntity Read<TEntity>(this IDbContext context, params object[] keyValues)
            where TEntity : class
        {
            return context.Set<TEntity>().Find(keyValues);
        }

        //public static TEntity GetOrCreate<TEntity>(this IDbContext context, int? id)
        //    where TEntity : class, new()
        //{
        //    if (id.HasValue)
        //        return Read<TEntity>(context, id.Value);

        //    var cc = new TEntity();
        //    Insert(context, cc);
        //    return cc;
        //}

        //public static TEntity GetOrCreate<TEntity>(this IDbContext context, int? id, IEnumerable<Expression<Func<TEntity, object>>> includes)
        //    where TEntity : class, new()
        //{
        //    return GetOrCreate(context, id, includes.ToArray());
        //}

        //public static TEntity GetOrCreate<TEntity>(this IDbContext context, int? id, params Expression<Func<TEntity, object>>[] includes)
        //    where TEntity : class, new()
        //{
        //    if (id.HasValue && id != 0)
        //        return ReadWithIncluded(context, id.Value, includes);

        //    var cc = new TEntity();
        //    Insert(context, cc);
        //    return cc;
        //}

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

        public static void FindAndDelete<TEntity>(this IDbContext context, params object[] keyValues)
            where TEntity : class
        {
            var existingEntity = context.Read<TEntity>(keyValues);
            Delete(context, existingEntity);
        }

        public static void FindAndDeleteAndSave<TEntity>(this IDbContext context, params object[] keyValues)
            where TEntity : class
        {
            var existingEntity = context.Read<TEntity>(keyValues);
            DeleteAndSave(context, existingEntity);
        }
    }
}