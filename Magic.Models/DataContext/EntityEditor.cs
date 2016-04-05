using Penna.Assessment.Models.Entities;
using Penna.Common.EqualityComparers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Magic.Models.DataContext;

namespace Penna.Assessment.Models.DataContext
{
    public static class DbContextExtensions
    {
        public static TEntity GetEntityByProperty<TEntity, TKey>(this DbContext context, Expression<Func<TEntity, TKey>> property, TKey keyValue)
            where TEntity : class
        {
            var query = Filter(context.Set<TEntity>(), property, keyValue);
            return query.FirstOrDefault();
        }


        private static IQueryable<TEntity> Filter<TEntity, TProperty>(IQueryable<TEntity> dbSet, Expression<Func<TEntity, TProperty>> property, TProperty value)
        {
            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null || !(memberExpression.Member is PropertyInfo))
            {
                throw new ArgumentException("Property expected", "property");
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(property.Body, Expression.Constant(value, typeof(TProperty))), property.Parameters.Single());

            return dbSet.Where(lambda);
        }

        public static EntityEditor<TEntity> Modify<TEntity>(this ApplicationDbContext context)
            where TEntity : class, IId, new()
        {
            return new EntityEditor<TEntity>(context);
        }

        public static EntityEditor<TEntity> SetPropertyIfNotNull<TEntity, TProp>(this EntityEditor<TEntity> editor, Expression<Func<TEntity, TProp>> propExpr, TProp value)
            where TEntity : class, IId, new()
        {
            return value != null ? editor.SetProperty(propExpr, value) : editor;
        }

        public class EntityEditor<TEntity> where TEntity : class, IId, new()
        {
            private readonly ApplicationDbContext _context;

            private readonly List<Action<TEntity>> _actions = new List<Action<TEntity>>();
            private readonly List<Expression<Func<TEntity, object>>> _includes = new List<Expression<Func<TEntity, object>>>();

            public EntityEditor(ApplicationDbContext context)
            {
                _context = context;
            }

            public EntityEditor<TEntity> SetProperty<TProp>(Expression<Func<TEntity, TProp>> propExpr, TProp value)
            {
                _actions.Add(x => propExpr.SetPropValue(x, value));
                return this;
            }

            public EntityEditor<TEntity> SetPropertyById<TProp>(Expression<Func<TEntity, TProp>> propExpr, int id)
                where TProp : class
            {
                _includes.Add(propExpr.ToObjectGetter());
                _actions.Add(x =>
                {
                    propExpr.SetPropValue(x, _context.Read<TProp>(id));
                });
                return this;
            }

            public EntityEditor<TEntity> SetPropertyByPredicate<TProp>(Expression<Func<TEntity, TProp>> propExpr, Expression<Func<TProp, bool>> conditionalExpression)
                where TProp : class
            {
                _includes.Add(propExpr.ToObjectGetter());
                _actions.Add(x =>
                {
                    propExpr.SetPropValue(x, _context.Set<TProp>().FirstOrDefault(conditionalExpression));
                });
                return this;
            }

            private static void UpdateCollection<TProp>(Expression<Func<TEntity, ICollection<TProp>>> propExpr, TEntity x, IEnumerable<TProp> collection)
                where TProp : class, IId
            {
                collection = collection ?? Enumerable.Empty<TProp>();
                var eq = Comparer.Build<TProp>().AddProjection(y => y.Id).Get();

                var orig = propExpr.Compile()(x);

                var a = new HashSet<TProp>(orig);
                var b = new HashSet<TProp>(collection);

                foreach (var y in a.Except(b, eq))
                {
                    orig.Remove(y);
                }
                foreach (var y in b.Except(a, eq))
                {
                    orig.Add(y);
                }
            }

            public EntityEditor<TEntity> SetCollection<TProp>(Expression<Func<TEntity, ICollection<TProp>>> propExpr,
                IEnumerable<TProp> collection)
                where TProp : class, IId
            {
                collection = collection ?? Enumerable.Empty<TProp>();
                _includes.Add(propExpr.ToObjectGetter());
                _actions.Add(x =>
                {
                    UpdateCollection(propExpr, x, collection);
                });
                return this;
            }

            public EntityEditor<TEntity> SetCollectionById<TProp>(Expression<Func<TEntity, ICollection<TProp>>> propExpr,
                IEnumerable<int> collection)
                where TProp : class, IId
            {
                collection = collection ?? Enumerable.Empty<int>();
                _includes.Add(propExpr.ToObjectGetter());
                _actions.Add(x =>
                {
                    UpdateCollection(propExpr, x, _context.Set<TProp>().Where(y => collection.Any(z => y.Id == z)));
                });
                return this;
            }

            public EntityEditor<TEntity> Add<TProp>(
                Expression<Func<TEntity, ICollection<TProp>>> propExpr,
                TProp entity)
                where TProp : class, IId
            {
                _includes.Add(propExpr.ToObjectGetter());
                _actions.Add(x =>
                {
                    propExpr.Compile()(x).Add(entity);
                });
                return this;
            }

            public EntityEditor<TEntity> AddById<TProp>(
                Expression<Func<TEntity, ICollection<TProp>>> propExpr,
                int id)
                where TProp : class, IId
            {
                _includes.Add(propExpr.ToObjectGetter());
                _actions.Add(x =>
                {
                    propExpr.Compile()(x).Add(_context.Read<TProp>(id));
                });
                return this;
            }

            public EntityEditor<TEntity> AddRange<TProp>(
                Expression<Func<TEntity, ICollection<TProp>>> propExpr,
                IEnumerable<TProp> entities)
                where TProp : class, IId
            {
                _includes.Add(propExpr.ToObjectGetter());
                _actions.Add(x =>
                {
                    var coll = propExpr.Compile()(x);
                    foreach (var e in entities)
                        coll.Add(e);
                });
                return this;
            }

            public EntityEditor<TEntity> AddRangeById<TProp>(
                Expression<Func<TEntity, ICollection<TProp>>> propExpr,
                IEnumerable<int> collection)
                where TProp : class, IId
            {
                _includes.Add(propExpr.ToObjectGetter());
                _actions.Add(x =>
                {
                    var coll = propExpr.Compile()(x);
                    foreach (var e in _context.Set<TProp>().Where(y => collection.Any(z => y.Id == z)))
                        coll.Add(e);
                });
                return this;
            }

            public EntityEditor<TEntity> SetStatus<TStatusEntity, TStatusEnum>(Expression<Func<TEntity, TStatusEntity>> propExpr, TStatusEnum status)
                where TStatusEntity : class, IEnum<TStatusEnum>
            {
                _actions.Add(x =>
                {
                    var statusEntity = _context.GetEntityByProperty<TStatusEntity, TStatusEnum>(e => e.Value, status);
                    propExpr.SetPropValue(x, statusEntity);
                });
                return this;
            }

            public TEntity SaveAndReturn(int? id)
            {
                var x = _context.GetOrCreate(id, _includes);
                _actions.ForEach(f => f(x));
                _context.SaveChanges();
                return x;
            }
        }
    }
}