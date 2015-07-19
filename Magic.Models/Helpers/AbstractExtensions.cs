using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Magic.Models.Helpers
{
    public abstract class AbstractExtensions
    {
        private static string viewModelNamespace = "Magic.Models"; // used only if entity framework proxies are passed to get ViewModels;

        public override string ToString()
        {
            var objName = GetType().FullName + ": ";
            var classMembers = GetType().GetProperties();

            return classMembers.Aggregate(objName, (toString, member) => toString + ("\n" + member.Name + " : " + member.GetValue(this) + "; "));
        }

        public string ToHtmlString()
        {
            var objName = GetType().FullName + ": ";
            var classMembers = GetType().GetProperties();

            var str = classMembers.Aggregate(objName, (toString, member) => toString + ("<br />" + member.Name + " : " + member.GetValue(this) + "; "));
            return WebUtility.HtmlDecode(str);
        }

        // Returns a new instance of the related viewModel.
        public IViewModel GetViewModel(params object[] args)
        {
            var viewModelType = GetType();
            var viewModelName = viewModelType.FullName;

            if (viewModelName.Contains("System.Data.Entity.DynamicProxies"))
            {
                viewModelName = viewModelName.Remove(viewModelName.LastIndexOf('_')).Replace("System.Data.Entity.DynamicProxies", viewModelType.BaseType.Namespace);
            }

            var viewModel = Type.GetType(viewModelName + "ViewModel");
            if (viewModel == null) 
                throw new NotImplementedException("No viewModel was found for model " + viewModelName + ". Please check if you have a viewModel class provided in the same namespace.");

            if (args.Length > 0)
            {
                return (IViewModel)Activator.CreateInstance(viewModel, this, args[0]);
            }
            return (IViewModel)Activator.CreateInstance(viewModel, this);
        }
    }

    public static class ExtensionMethods
    {
        public static string ToFullString(this object obj)
        {
            var objName = obj.GetType().FullName + ": ";
            var classMembers = obj.GetType().GetProperties();

            return classMembers.Aggregate(objName, (toString, member) => toString + ("\n" + member.Name + " : " + member.GetValue(obj) + "; "));
        }

        public static string AssignRandomColorCode(this string str)
        {
            var random = new Random();
            var red = random.Next(256);
            var green = random.Next(256);
            var blue = random.Next(256);
            var color = System.Drawing.Color.FromArgb(red, green, blue);

            return ColorTranslator.ToHtml(color);
        }

        public static string ToTotalHoursString(this TimeSpan timeSpan)
        {
            var str = timeSpan.ToString("hh\\:mm\\:ss");
            if (timeSpan.Days > 0)
            {
                str = str.Remove(0, 2).Insert(0, (timeSpan.Days * 24 + timeSpan.Hours).ToString());
            }
            return str;
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            var display = enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttribute<DisplayAttribute>();
            return display != null ? display.Name : enumValue.ToString();
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            var elements = source.ToArray();
            for (var i = elements.Length - 1; i >= 0; i--)
            {
                var swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        //public static string RenderPartialToString(string viewName, object model)
        //{
        //    var viewPage = new ViewPage
        //    {
        //        ViewContext = new ViewContext(), 
        //        ViewData = new ViewDataDictionary(model)
        //    };

        //    viewPage.Controls.Add(viewPage.LoadControl(viewName));

        //    var sb = new StringBuilder();
        //    using (var sw = new StringWriter(sb))
        //    {
        //        using (var tw = new HtmlTextWriter(sw))
        //        {
        //            viewPage.RenderControl(tw);
        //        }
        //    }

        //    return sb.ToString();
        //}
    }
}