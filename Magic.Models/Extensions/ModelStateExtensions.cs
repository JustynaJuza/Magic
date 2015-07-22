using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Magic.Models.Extensions
{
    public static class ModelStateExtensions
    {
        public static void RemoveFor<TModel>(this ModelStateDictionary modelState,
                                         Expression<Func<TModel, object>> expression)
        {
            string expressionText = ExpressionHelper.GetExpressionText(expression);

            foreach (var ms in modelState.ToArray())
            {
                if (ms.Key.StartsWith(expressionText + "."))
                {
                    modelState.Remove(ms);
                }
            }
        }
    }
}