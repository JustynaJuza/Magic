using System.Linq;

namespace Juza.Magic.Models.Projections
{
    /// <summary>
    /// A specialised type of Mapping that stays within the Queryable paradigm
    /// (i.e. can be entirely translated into SQL by EF)
    /// </summary>
    public interface IQueryMapping<in TSource, out TDest>
    {
        IQueryable<TDest> Apply(IQueryable<TSource> source);
    }

    public interface IObjectMapping<in TSource, out TDest> : IQueryMapping<TSource, TDest>
    {
        TDest Apply(TSource source);
    }
}