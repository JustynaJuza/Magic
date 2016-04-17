using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Juza.Magic.Models.DataContext
{
    public static class EntityExtensions
    {
        public static Expression<Func<TEntity, bool>> MatchKey<TEntity>(string[] keyPropertyNames, params object[] keyValue)
            where TEntity : class
        {
            if (keyValue.Length != keyPropertyNames.Length)
            {
                throw new ArgumentException(
                    string.Format(
                        "Entity type {0} has a key composed of {1} value(s) but the query was called with {2} argument(s) supplied",
                        typeof(TEntity),
                        keyPropertyNames.Length,
                        keyValue.Length));
            }

            var match = PredicateExtensions.Begin<TEntity>(true);

            for (var i = 0; i < keyPropertyNames.Length; i++)
            {
                match = match.And(PropertyHasValue<TEntity>(keyPropertyNames[i], keyValue[i]));
            }

            return match;
        }

        public static Expression<Func<TEntity, bool>> PropertyHasValue<TEntity>(string propertyName, object propertyValue)
            where TEntity : class
        {
            var property = typeof(TEntity).GetProperty(propertyName);
            return PropertyEquals<TEntity, object>(property, propertyValue);
        }

        public static Expression<Func<TEntity, bool>> PropertyEquals<TEntity, TValue>(PropertyInfo property, TValue value)
        {
            var param = Expression.Parameter(typeof(TEntity));
            var body = Expression.Equal(Expression.Property(param, property), Expression.Constant(value));
            return Expression.Lambda<Func<TEntity, bool>>(body, param);
        }
    }
}