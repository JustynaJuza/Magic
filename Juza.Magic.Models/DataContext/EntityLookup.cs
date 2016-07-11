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
        public IEntityLookup<TEntity> Include(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                var navigationPropertyType = ((PropertyInfo) memberExpression.Member).PropertyType;
                var isReferenceProperty = navigationPropertyType.IsGenericType &&
                                          navigationPropertyType.IsAssignableFrom(typeof(object));

                var entityParam = Expression.Parameter(typeof(TEntity));
                if (isReferenceProperty)
                {
                    var objectExpression = Expression.Convert(memberExpression, typeof(object));
                    var referenceExpression = Expression.Lambda<Func<TEntity, object>>(objectExpression, entityParam);
                    ReferenceExpressions.Add(referenceExpression);
                }
                else
                {
                    var objectExpression = Expression.Convert(memberExpression, typeof(ICollection<object>));
                    var collectionMember = Expression.Lambda<Func<TEntity, ICollection<object>>>(objectExpression, entityParam);
                    CollectionExpressions.Add(collectionMember);
                }
            }


            //IncludeNestedEntitiesInReference(memberExpression);
            return this;
        }

        public IEntityLookup<TEntity> Include(Expression<Func<TEntity, object>> expression)
        {
            IncludeEntities(expression);
            return this;
        }

        public IEntityLookup<TEntity> Include(Expression<Func<TEntity, ICollection<object>>> expression)
        {
            IncludeEntities(expression);
            return this;
        }


        private void IncludeEntities(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                AddNavigationProperty(memberExpression);
            }

            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null)
            {
                IncludeSelectedEntities(methodCallExpression);
            }

            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression != null)
            {
                IncludeNestedEntities(lambdaExpression);
            }
        }

        public void AddNavigationProperty(MemberExpression navigationProperty)
        {
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
        }

        private void IncludeNestedEntities(LambdaExpression expression)
        {
            var methodCallExpression = expression.Body as MethodCallExpression;
            if (methodCallExpression != null)
            {
                IncludeSelectedEntities(methodCallExpression);
            }

            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression != null)
            {
                AddNavigationProperty(memberExpression);
                IncludeEntities(memberExpression.Expression);
            }
        }

        private void IncludeSelectedEntities(MethodCallExpression expression)
        {
            if (expression == null) return;

            var isValidSelectExpression = expression.Method.Name == "Select" && expression.Arguments.Count == 2;
            if (!isValidSelectExpression)
            {
                throw new ArgumentException("The Include path expression must refer to a navigation property defined on the type." +
                                            "Use the Select operator for including nested navigation properties.");
            }

            AddNavigationProperty((MemberExpression) expression.Arguments[0]);

            // include further entities in nested selects
            IncludeEntities((LambdaExpression) expression.Arguments[1]);

            //var navigationProperty = (MemberExpression) expression.Arguments[0];
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
            //var navigationProperty = expression.Arguments[0];
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