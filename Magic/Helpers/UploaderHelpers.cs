using System;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Magic.Helpers
{
    public static class UploaderHelpers
    {
        private static readonly string PlaceholderImage = VirtualPathUtility.ToAbsolute("~/Content/placeholder.png");

        public static string GetIdFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            return helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
        }
        
        public static string GetPlaceholder(this HtmlHelper helper)
        {
            return PlaceholderImage;
        }
    }
}