using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Returns a new instance of the related viewModel.
        public IViewModel GetViewModel()
        {
            string viewModelName = this.GetType().FullName + "ViewModel ";
            var viewModel = Type.GetType(viewModelName);
            //Convert.ChangeType(Activator.CreateInstance(viewModel, this), viewModel);
            return (IViewModel) Activator.CreateInstance(viewModel, this);
        }
    }

    public static class ObjectExtension
    {
        public static string ToFullString(this object myObj)
        {
            string toString = myObj.GetType().FullName + ": ";
            var classMembers = myObj.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(myObj) + "; ";

            return toString;
        }
    }
}