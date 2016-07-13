using System.Collections.Generic;
using System.Linq;

namespace Juza.Magic.Models.Projections
{
    public interface IMapping<in TSource, out TDest>
    {
    }

    /// <summary>
    /// A specialised type of Mapping that stays within the Queryable paradigm
    /// (i.e. can be entirely translated into SQL by EF)
    /// </summary>
    public interface IQueryMapping<in TSource, out TDest>
    {
        IQueryable<TDest> ApplyAsQuery(IQueryable<TSource> source);
    }

    /// <summary>
    /// A mapping from one type to another, with the ability to map from a Queryable set
    /// to an Enumerable result set
    /// </summary>
    public interface ICollectionMapping<in TSource, out TDest> : IQueryMapping<TSource, TDest>
    {
        IEnumerable<TDest> Apply(IQueryable<TSource> source);
    }

    public interface IObjectMapping<in TSource, out TDest> : ICollectionMapping<TSource, TDest>
    {
        TDest Apply(TSource source);
    }
}