using System;
using System.Linq.Expressions;

namespace Magic.Models.DataContext
{
    public static class EntityExtensions
    {
        public static Expression<Func<TEntity, bool>> MatchKey<TEntity>(params object[] keyValues)
            where TEntity : class
        {
            var keyProperties = keyValues.GetType().GetProperties();
            var match = PredicateExtensions.Begin<TEntity>(true);

            foreach (var keyProperty in keyProperties)
            {
                var keyPropertyValue = keyProperty.GetValue(keyValues);

                match = match.And(e => keyPropertyValue.Equals(GetPropertyValue<TEntity>(keyProperty.Name)));
            }

            return match;
        }

        public static Expression<Func<TEntity, object>> GetPropertyValue<TEntity>(string propertyName)
            where TEntity : class
        {
            return e => e.GetType().GetProperty(propertyName).GetValue(e);
        }
    }
}