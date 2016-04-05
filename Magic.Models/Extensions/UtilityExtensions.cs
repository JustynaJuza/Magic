using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace Magic.Models.Extensions
{
    public static class UtilityExtensions
    {

        public static string ToFullString(this object obj)
        {
            var objName = obj.GetType().FullName + ": ";
            var classMembers = obj.GetType().GetProperties();

            return classMembers.Aggregate(objName, (toString, member) => toString + ("\n" + member.Name + " : " + member.GetValue(obj) + "; "));
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            var display = enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttribute<DisplayAttribute>();
            return display != null ? display.Name : enumValue.ToString();
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

        public static object GetDefaultValue(this Type type)
        {
            // using !type.IsValueType to cope with interfaces
            return !type.IsValueType ? null : Activator.CreateInstance(type);
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