using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Magic.Helpers
{
    public static class HtmlHelperExtensions
    {
        private const string _jSViewDataName = "RenderJavaScript";
        private const string _styleViewDataName = "RenderStyle";

        public static void AddJavaScript(this HtmlHelper htmlHelper,
                                         string scriptURL)
        {
            List<string> scriptList = htmlHelper.ViewContext.HttpContext
              .Items[HtmlHelperExtensions._jSViewDataName] as List<string>;
            if (scriptList != null)
            {
                if (!scriptList.Contains(scriptURL))
                {
                    scriptList.Add(scriptURL);
                }
            }
            else
            {
                scriptList = new List<string>();
                scriptList.Add(scriptURL);
                htmlHelper.ViewContext.HttpContext
                  .Items.Add(HtmlHelperExtensions._jSViewDataName, scriptList);
            }
        }

        public static MvcHtmlString RenderJavaScripts(this HtmlHelper HtmlHelper)
        {
            StringBuilder result = new StringBuilder();

            List<string> scriptList = HtmlHelper.ViewContext.HttpContext
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

        public static void AddStyle(this HtmlHelper htmlHelper, string styleURL)
        {
            List<string> styleList = htmlHelper.ViewContext.HttpContext
              .Items[HtmlHelperExtensions._styleViewDataName] as List<string>;

            if (styleList != null)
            {
                if (!styleList.Contains(styleURL))
                {
                    styleList.Add(styleURL);
                }
            }
            else
            {
                styleList = new List<string>();
                styleList.Add(styleURL);
                htmlHelper.ViewContext.HttpContext
                  .Items.Add(HtmlHelperExtensions._styleViewDataName, styleList);
            }
        }

        public static MvcHtmlString RenderStyles(this HtmlHelper htmlHelper)
        {
            StringBuilder result = new StringBuilder();

            List<string> styleList = htmlHelper.ViewContext.HttpContext
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
    }
}