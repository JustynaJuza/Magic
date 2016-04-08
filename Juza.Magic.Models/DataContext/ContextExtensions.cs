using System.Linq;

namespace Juza.Magic.Models.DataContext
{
    public static partial class ContextExtensions
    {
        public static bool Exists<TEntity>(this IDbContext context, TEntity entity)
            where TEntity : class
        {
            return context.Set<TEntity>().Local.Any(e => e == entity);
        }
        public static bool Exists<TEntity>(this IDbContext context, params object[] keyValues)
            where TEntity : class
        {
            return context.Set<TEntity>().Local.Any(EntityExtensions.MatchKey<TEntity>(keyValues).Compile());
        }
    }
}