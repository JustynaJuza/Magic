using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Juza.Magic.Models.DataContext
{
    public interface IEntityLookup<TEntity>
    {
        IList<Expression<Func<TEntity, ICollection<object>>>> CollectionExpressions { get; set; }
        IList<Expression<Func<TEntity, object>>> ReferenceExpressions { get; set; }

        IEntityLookup<TEntity> Include(Expression<Func<TEntity, object>> memberExpression);
        IEntityLookup<TEntity> Include(Expression<Func<TEntity, ICollection<object>>> memberExpression);
        TEntity FindOrFetchEntity(params object[] keyValues);
    }

    public class EntityLookup<TEntity> : IEntityLookup<TEntity>
    {
        public delegate TEntity Callback(IEntityLookup<TEntity> entityLookup, params object[] keyValues);

        private readonly Callback _findOrFetchEntity;
        public IList<Expression<Func<TEntity, ICollection<object>>>> CollectionExpressions { get; set; }
        public IList<Expression<Func<TEntity, object>>> ReferenceExpressions { get; set; }

        public EntityLookup(Callback findOrFetchEntity)
        {
            _findOrFetchEntity = findOrFetchEntity;
            CollectionExpressions = new List<Expression<Func<TEntity, ICollection<object>>>>();
            ReferenceExpressions = new List<Expression<Func<TEntity, object>>>();
        }

        public IEntityLookup<TEntity> Include(Expression<Func<TEntity, object>> memberExpression)
        {
            IncludeNestedEntitiesInReference(memberExpression);
            ReferenceExpressions.Add(memberExpression);
            return this;
        }

        public IEntityLookup<TEntity> Include(Expression<Func<TEntity, ICollection<object>>> memberExpression)
        {
            IncludeNestedEntitiesInCollection(memberExpression);
            CollectionExpressions.Add(memberExpression);
            return this;
        }

        private void IncludeNestedEntitiesInCollection(Expression<Func<TEntity, ICollection<object>>> memberExpression)
        {
            var methodCallExpression = memberExpression.Body as MethodCallExpression;
            IncludeNestedEntities(methodCallExpression);
        }

        private void IncludeNestedEntitiesInReference(Expression<Func<TEntity, object>> memberExpression)
        {
            var methodCallExpression = memberExpression.Body as MethodCallExpression;
            IncludeNestedEntities(methodCallExpression);
        }

        private void IncludeNestedEntities(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression == null) return;

            var isValidSelectExpression = methodCallExpression.Method.Name == "Select" && methodCallExpression.Arguments.Count == 2;
            if (!isValidSelectExpression)
            {
                throw new ArgumentException("The Include path expression must refer to a navigation property defined on the type." +
                                            "Use the Select operator for including nested navigation properties.");
            }

            var navigationProperty = (MemberExpression) methodCallExpression.Arguments[0];
            var navigationPropertyType = ((PropertyInfo) navigationProperty.Member).PropertyType;
            var isCollectionProperty = navigationPropertyType.IsGenericType
                                       && navigationPropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);

            var entityParam = Expression.Parameter(typeof(TEntity), "x");
            if (isCollectionProperty)
            {
                var objectExpression = Expression.Convert(navigationProperty, typeof(ICollection<object>));
                var collectionMember = Expression.Lambda<Func<TEntity, ICollection<object>>>(objectExpression, entityParam);
                CollectionExpressions.Add(collectionMember);
            }
            else
            {
                var referenceMember = Expression.Lambda<Func<TEntity, object>>(navigationProperty, entityParam);
                ReferenceExpressions.Add(referenceMember);
            }

            // include further entities in nested selects
            IncludeNestedEntities(methodCallExpression.Arguments[1] as MethodCallExpression);

            //var navigationProperty = (MemberExpression) methodCallExpression.Arguments[0];
            //var navigationPropertyInfo = ((PropertyInfo) navigationProperty.Member);
            //var navigationPropertyType = navigationPropertyInfo.PropertyType;

            //var isCollectionProperty = navigationPropertyType.IsGenericType
            //    && navigationPropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);

            //var entityParam = Expression.Parameter(typeof(TEntity));
            //if (isCollectionProperty)
            //{
            //    var actualMemberExpression = Expression.Property(entityParam, navigationPropertyInfo);
            //    var collectionMember = Expression.Lambda<Func<TEntity, ICollection<object>>>(actualMemberExpression, entityParam);
            //    CollectionExpressions.Add(collectionMember);
            //}
            //else
            //{
            //    var referenceMember = Expression.Lambda<Func<TEntity, object>>(navigationProperty, entityParam);
            //    ReferenceExpressions.Add(referenceMember);
            //}

            // process rest of expression

            //// parse first argument
            //var navigationProperty = methodCallExpression.Arguments[0];
            //var isCollectionProperty = navigationProperty.GetType().IsAssignableFrom(typeof(ICollection<object>));

            //var entityParam = Expression.Parameter(typeof(TEntity), "x");
            //if (isCollectionProperty)
            //{
            //    var collectionMember = Expression.Lambda<Func<TEntity, ICollection<object>>>(navigationProperty, entityParam);
            //    CollectionExpressions.Add(collectionMember);
            //}
            //else
            //{
            //    var referenceMember = Expression.Lambda<Func<TEntity, object>>(navigationProperty, entityParam);
            //    ReferenceExpressions.Add(referenceMember);
            //}

            //// process rest of expression
        }

        public TEntity FindOrFetchEntity(params object[] keyValues)
        {
            return _findOrFetchEntity(this, keyValues);
        }

    }
}