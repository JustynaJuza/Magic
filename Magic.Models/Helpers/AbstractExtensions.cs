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
            string toString = this.GetType().FullName + ": ";
            var classMembers = this.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(this) + "; ";

            return toString;
        }

        public string ToHtmlString()
        {
            var toString = GetType().FullName + ": ";
            var classMembers = GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "<br />" + member.Name + " : " + member.GetValue(this) + "; ";

            return WebUtility.HtmlDecode(toString);
        }

        // Returns a new instance of the related viewModel.
        public IViewModel GetViewModel(params object[] args)
        {
            string viewModelName = GetType().FullName;

            if (viewModelName.Contains("System.Data.Entity.DynamicProxies"))
            {
                viewModelName = viewModelName.Remove(viewModelName.LastIndexOf('_')).Replace("System.Data.Entity.DynamicProxies", viewModelNamespace);
            }

            var viewModel = Type.GetType(viewModelName + "ViewModel");
            //Convert.ChangeType(Activator.CreateInstance(viewModel, this), viewModel);
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
            var toString = obj.GetType().FullName + ": ";
            var classMembers = obj.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(obj) + "; ";

            return toString;
        }

        public static string AssignRandomColorCode(this string str)
        {
            var random = new Random();
            var red = random.Next(256);
            var green = random.Next(256);
            var blue = random.Next(256);
            var color = System.Drawing.Color.FromArgb(red, green, blue);

            return System.Drawing.ColorTranslator.ToHtml(color);
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
    }
}