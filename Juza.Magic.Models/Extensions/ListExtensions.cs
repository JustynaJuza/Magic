
using System.Collections.Generic;
using System.Linq;

namespace Juza.Magic.Models.Extensions
{
    public static class ListExtensions
    {
        //    public static IList<SelectListItem> GetSelectList<T>(this IEnumerable<T> list) where T : IEntity
        //    {
        //        return new List<SelectListItem>(list.Select(item => new SelectListItem { Text = item.Name, Value = item.Id.ToString() }));
        //    }

        //    public static IOrderedEnumerable<SelectListItem> Order(this IEnumerable<SelectListItem> list)
        //    {
        //        return list.OrderBy(x => x.Text);
        //    }
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}