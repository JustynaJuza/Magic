using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Juza.Magic.Models.Projections
{
    /// <summary>
    /// Encapsulates a mapping from a database entity to a specified object (probably DTO).
    /// </summary>
    public class ObjectMapping<TSource, TDest> : IObjectMapping<TSource, TDest>
    {
        private readonly Expression<Func<TSource, TDest>> _projection;

        public ObjectMapping(Expression<Func<TSource, TDest>> projection)
        {
            _projection = projection;
        }

        public TDest Apply(TSource source)
        {
            return _projection.Compile().Invoke(source);
        }

        IEnumerable<TDest> ICollectionMapping<TSource, TDest>.Apply(IQueryable<TSource> source)
        {
            return ApplyAsQuery(source);
        }

        public IQueryable<TDest> ApplyAsQuery(IQueryable<TSource> source)
        {
            return source.Select(_projection);
        }
    }
}