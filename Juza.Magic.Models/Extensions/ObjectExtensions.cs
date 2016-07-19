using System.Linq;

namespace Juza.Magic.Models.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToContentString(this object model)
        {
            var modelType = model.GetType();
            var objName = modelType.FullName + ": ";
            var classMembers = modelType.GetProperties();

            return classMembers.Aggregate(objName, (toString, member) => toString + ("\n" + member.Name + " : " + member.GetValue(model) + "; "));
        }

        public static string ToHtmlString(this object model)
        {
            return ToContentString(model).Replace("\n", "<br />");
            //return WebUtility.HtmlEncode(ToContentString(model).Replace("\n", "<br />"));
        }
    }
}