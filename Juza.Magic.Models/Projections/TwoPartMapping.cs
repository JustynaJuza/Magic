using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Juza.Magic.Models.Projections
{
    /// <summary>
    /// Encapsulates a mapping that is performed in two stages.
    /// This is useful for projecting to an intermediate type within an Entity Framework query,
    /// then performing a further projection to the destination type in-memory. This allows
    /// the user to return a minimal results set from the query while still ending up with
    /// a correctly typed object (eg using ToNullableEnum() which cannot be translated by EF)
    /// </summary>
    public class TwoPartMapping<TSource, TIntermediate, TDest> : IObjectMapping<TSource, TDest>
    {
        private readonly Expression<Func<TSource, TIntermediate>> _projection;
        private readonly Func<TIntermediate, TDest> _postProcess;
        private readonly Lazy<Func<TSource, TIntermediate>> _compiled;

        public TwoPartMapping(Expression<Func<TSource, TIntermediate>> projection, Func<TIntermediate, TDest> postProcess)
        {
            _projection = projection;
            _postProcess = postProcess;
            _compiled = new Lazy<Func<TSource, TIntermediate>>(_projection.Compile);
        }

        public IEnumerable<TDest> Apply(IQueryable<TSource> source)
        {
            return source.Select(_projection).Select(_postProcess);
        }
    }
}