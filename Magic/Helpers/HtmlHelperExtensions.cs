using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;

namespace Magic.Helpers
{
    public static class HtmlHelperExtensions
    {
        private const string _jSViewDataName = "RenderJavaScript";
        private const string _styleViewDataName = "RenderStyle";

        public static void AddJavaScript(this HtmlHelper htmlHelper, string scriptUrl)
        {
            var scriptList = htmlHelper.ViewContext.HttpContext
              .Items[HtmlHelperExtensions._jSViewDataName] as List<string>;
            if (scriptList != null)
            {
                if (!scriptList.Contains(scriptUrl))
                {
                    scriptList.Add(scriptUrl);
                }
            }
            else
            {
                scriptList = new List<string> { scriptUrl };
                htmlHelper.ViewContext.HttpContext
                  .Items.Add(HtmlHelperExtensions._jSViewDataName, scriptList);
            }
        }

        public static MvcHtmlString RenderJavaScripts(this HtmlHelper htmlHelper)
        {
            var result = new StringBuilder();

            var scriptList = htmlHelper.ViewContext.HttpContext
              .Items[HtmlHelperExtensions._jSViewDataName] as List<string>;
            if (scriptList != null)
            {
                foreach (string script in scriptList)
                {
                    result.AppendLine(string.Format(
                      "<script type=\"text/javascript\" src=\"{0}\"></script>",
                      script));
                }
            }

            return MvcHtmlString.Create(result.ToString());
        }

        public static void AddStyle(this HtmlHelper htmlHelper, string styleUrl)
        {
            var styleList = htmlHelper.ViewContext.HttpContext
              .Items[HtmlHelperExtensions._styleViewDataName] as List<string>;

            if (styleList != null)
            {
                if (!styleList.Contains(styleUrl))
                {
                    styleList.Add(styleUrl);
                }
            }
            else
            {
                styleList = new List<string>();
                styleList.Add(styleUrl);
                htmlHelper.ViewContext.HttpContext
                  .Items.Add(HtmlHelperExtensions._styleViewDataName, styleList);
            }
        }

        public static MvcHtmlString RenderStyles(this HtmlHelper htmlHelper)
        {
            var result = new StringBuilder();

            var styleList = htmlHelper.ViewContext.HttpContext
              .Items[HtmlHelperExtensions._styleViewDataName] as List<string>;

            if (styleList != null)
            {
                foreach (string script in styleList)
                {
                    result.AppendLine(string.Format(
                      "<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />",
                      script));
                }
            }

            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString Image(this HtmlHelper helper, string src, string altText, string height)
        {
            var builder = new TagBuilder("img");
            builder.MergeAttribute("src", src);
            builder.MergeAttribute("alt", altText);
            builder.MergeAttribute("height", height);
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString DisplayWithIdFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string wrapperTag = "div")
        {
            var id = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
            return MvcHtmlString.Create(string.Format("<{0} id=\"{1}\">{2}</{0}>", wrapperTag, id, helper.DisplayFor(expression)));
        }

        public static MvcHtmlString ActionLinkElement(this HtmlHelper helper, MvcHtmlString helperString, string innerElement)
        {
            var rawHelperString = helperString.ToString();

            string leftTag = rawHelperString.Substring(0, rawHelperString.IndexOf(">") + 1);
            string rightTag = rawHelperString.Substring(rawHelperString.LastIndexOf("<"));

            return MvcHtmlString.Create(leftTag + innerElement + rightTag);
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
{
    // Get the type of the property.
    var typeOfProperty = expression.ReturnType;
    if (!typeOfProperty.IsEnum)
    {
        throw new ArgumentException(string.Format("Type {0} is not an enum", typeOfProperty));
    }
 
    // Get all the enum values.
    var values = Enum.GetValues(typeOfProperty);
  
    // Create a dictionary of the enum values.
    var items = values.Cast<object>().ToDictionary(key => key, value => value.ToString());
 
    // Get the metadata for the expression and ViewData of the current view.
    var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
 
    // Create a select list from the dictionary and use the default DropDownListFor method to create a drop down list.
    return htmlHelper.DropDownListFor(expression, new SelectList(items, "Key", "Value", metadata.Model));
}

        //public static string GetIdFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        //{
        //    return helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
        //}

        //public static string GetPlaceholder(this HtmlHelper helper)
        //{
        //    return placeholderImage;
        //}
    }
}