using System;
using System.Linq.Expressions;

namespace Juza.Magic.Models.Projections
{
    /// <summary>
    /// Hosts methods for producing instances of IMapping and IQueryMapping.
    /// Uses internal builder classes to allow anonymous types to be used as
    /// intermediates (through type inference).
    /// 
    /// In the other partial files of this class, there are static factory methods for producing
    /// mappings that cannot be expressed with the type system (ie they use intermediate anonymous
    /// objects to construct the destination type)
    /// </summary>
    public static class MappingFactory
    {
        /// <summary>
        /// Helper class for building mappings with anonymous types
        /// </summary>
        class MapSource<TSource>
        {
            public IQueryMapping<TSource, TDest> To<TDest>(Expression<Func<TSource, TDest>> projection)
            {
                return new SingleMapping<TSource, TDest>(projection);
            }

            public MapIntermediate<TSource, TIntermediate> ToIntermediate<TIntermediate>(Expression<Func<TSource, TIntermediate>> projection)
            {
                return new MapIntermediate<TSource, TIntermediate>(projection);
            }
        }

        /// <summary>
        /// Helper class for building mappings with anonymous types
        /// </summary>
        class MapIntermediate<TSource, TIntermediate>
        {
            private readonly Expression<Func<TSource, TIntermediate>> _projection;

            public MapIntermediate(Expression<Func<TSource, TIntermediate>> projection)
            {
                _projection = projection;
            }

            public IMapping<TSource, TDest> To<TDest>(Func<TIntermediate, TDest> postProcess)
            {
                return new TwoPartMapping<TSource, TIntermediate, TDest>(_projection, postProcess);
            }
        }

        /// <summary>
        /// Entry point into builder classes
        /// </summary>
        private static MapSource<TSource> From<TSource>()
        {
            return new MapSource<TSource>();
        }
    }
}
