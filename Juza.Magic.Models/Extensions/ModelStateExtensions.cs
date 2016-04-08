using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.ModelBinding;

namespace Juza.Magic.Models.Extensions
{
    public static class ModelStateExtensions
    {
        public static void RemoveFor<TModel>(this ModelStateDictionary modelState,
                                         Expression<Func<TModel, object>> expression)
        {
            string propertyName = ((MemberExpression) expression.Body).Member.Name;

            foreach (var ms in modelState.ToArray())
            {
                if (ms.Key.StartsWith(propertyName + "."))
                {
                    modelState.Remove(ms);
                }
            }
        }
    }
}