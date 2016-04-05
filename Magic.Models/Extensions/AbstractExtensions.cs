using System;
using System.Linq;
using System.Net;
using Magic.Models.Interfaces;

namespace Magic.Models.Extensions
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
}