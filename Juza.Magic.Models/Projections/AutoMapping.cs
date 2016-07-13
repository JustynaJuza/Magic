using AutoMapper;
using AutoMapper.QueryableExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Juza.Magic.Models.Projections
{
    /// <summary>
    /// An AutoMapper adapter for existing AutoMapper configurations, plus simple new mappings
    /// </summary>
    public class AutoMapping<TSource, TDest> : IObjectMapping<TSource, TDest>
    {
        private readonly IConfigurationProvider _mappingConfiguration;

        public AutoMapping(Func<IMappingExpression<TSource, TDest>, IMappingExpression<TSource, TDest>> configure)
        {
            _mappingConfiguration = new MapperConfiguration(cfg =>
            {
                configure(cfg.CreateMap<TSource, TDest>());
            });
        }

        public IEnumerable<TDest> Apply(IQueryable<TSource> source)
        {
            return source.ProjectTo<TDest>(_mappingConfiguration);
        }

        public IQueryable<TDest> ApplyAsQuery(IQueryable<TSource> source)
        {
            return source.ProjectTo<TDest>(_mappingConfiguration);
        }

        public TDest Apply(TSource source)
        {
            var mapper = _mappingConfiguration.CreateMapper();
            return mapper.Map<TSource, TDest>(source);
        }
    }

    /// <summary>
    /// The default mapper that is used if no other exists.
    /// Saves on creating lots of small repetitive mappings
    /// </summary>
    public class DefaultAutoMapping<TSource, TDest> : AutoMapping<TSource, TDest>
    {
        public DefaultAutoMapping()
            : base(x => x) { }
    }
}
