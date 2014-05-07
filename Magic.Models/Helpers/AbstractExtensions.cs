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
            string toString = this.GetType().FullName + ": ";
            var classMembers = this.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "<br />" + member.Name + " : " + member.GetValue(this) + "; ";

            return WebUtility.HtmlDecode(toString);
        }

        // Returns a new instance of the related viewModel.
        public IViewModel GetViewModel()
        {
            string viewModelName = this.GetType().FullName + "ViewModel ";
            var viewModel = Type.GetType(viewModelName);
            //Convert.ChangeType(Activator.CreateInstance(viewModel, this), viewModel);
            return (IViewModel) Activator.CreateInstance(viewModel, this);
        }
    }

    public static class ExtensionMethods
    {
        public static string ToFullString(this object obj)
        {
            string toString = obj.GetType().FullName + ": ";
            var classMembers = obj.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(obj) + "; ";

            return toString;
        }

        public static void AssignRandomColorCode(this string str)
        {
            Random random = new Random();
            int red = random.Next(255); // Not 256, because black is the system message color.
            int green = random.Next(255);
            int blue = random.Next(255);
            System.Drawing.Color color = System.Drawing.Color.FromArgb(red, green, blue);

            str = System.Drawing.ColorTranslator.ToHtml(color);
        }
    }
}