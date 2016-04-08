using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
            ReferenceExpressions.Add(memberExpression);
            return this;
        }

        public IEntityLookup<TEntity> Include(Expression<Func<TEntity, ICollection<object>>> memberExpression)
        {
            CollectionExpressions.Add(memberExpression);
            return this;
        }

        public TEntity FindOrFetchEntity(params object[] keyValues)
        {
            return _findOrFetchEntity(this, keyValues);
        }
    }
}