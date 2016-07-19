using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Penna.Assessment.Extensions
{
    public static partial class HtmlExtensions
    {
        public static IHtmlString MultipleSelectListFor<TModel, TProperty>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression,
            string listRequestUrl)
            where TProperty : IEnumerable<int>
        {
            return MultipleSelectListFor(helper, expression, listRequestUrl, null);
        }

        public static IHtmlString MultipleSelectListFor<TModel, TProperty>(
           this HtmlHelper<TModel> helper,
           Expression<Func<TModel, TProperty>> expression,
           string listRequestUrl,
           object htmlAttributes)
           where TProperty : IEnumerable<int>
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return MultipleSelectListFor(helper, expression, listRequestUrl, attributes);
        }

        public static IHtmlString MultipleSelectListFor<TModel, TProperty>(
           this HtmlHelper<TModel> helper,
           Expression<Func<TModel, TProperty>> expression,
           string listRequestUrl,
           IDictionary<string, object> htmlAttributes)
           where TProperty : IEnumerable<int>
        {
            var settings = new SelectListSettings
            {
                Url = listRequestUrl
            };
            return MultipleSelectListFor(helper, expression, settings, htmlAttributes);
        }

        public static IHtmlString MultipleSelectListFor<TModel, TProperty>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression,
            SelectListSettings settings)
            where TProperty : IEnumerable<int>
        {
            return MultipleSelectListFor(helper, expression, settings, null);
        }

        public static IHtmlString MultipleSelectListFor<TModel, TProperty>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression,
            SelectListSettings settings,
            object htmlAttributes)
            where TProperty : IEnumerable<int>
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return MultipleSelectListFor(helper, expression, settings, attributes);
        }

        public static IHtmlString MultipleSelectListFor<TModel, TProperty>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression,
            SelectListSettings settings,
            IDictionary<string, object> htmlAttributes)
            where TProperty : IEnumerable<int>
        {
            var memberName = ExpressionHelper.GetExpressionText(expression);
            var id = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(memberName);
            var name = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(memberName);
            var currentSelection = (IEnumerable) ModelMetadata.FromLambdaExpression(expression, helper.ViewData).Model;

            return helper.Partial("~/Views/Shared/_MultipleSelectListPartial.cshtml",
                new MultipleSelectList
                {
                    ElementId = id,
                    ElementName = name,
                    CurrentSelection = currentSelection ?? Enumerable.Empty<object>(),
                    Settings = settings,
                    Attributes = htmlAttributes ?? new Dictionary<string, object>()
                });
        }
    }

    public class MultipleSelectList
    {
        public SelectListType ListType { get; }
        public string ElementId { get; set; }
        public string ElementName { get; set; }
        public IEnumerable CurrentSelection { get; set; }
        public SelectListSettings Settings { get; set; }
        public IDictionary<string, object> Attributes { get; set; }

        public MultipleSelectList()
        {
            ListType = SelectListType.Multiple;
        }
    }

    public enum SelectListType
    {
        Single,
        Multiple
    }

    public class SelectListSettings
    {
        public string Label { get; set; }
        public bool IsLabelDisabled { get; set; }
        public string Url { get; set; }
        public string LoadingElementId { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public string Callback { get; set; }
        public string OnChange { get; set; }
    }
}