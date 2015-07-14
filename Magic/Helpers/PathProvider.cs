using System.Web;
using System.Web.Hosting;
using Microsoft.Ajax.Utilities;

namespace Magic.Helpers
{
    public interface IPathProvider
    {
        string GetAppBaseUrl();
        string ConvertToDirectImageUrl(string url);
        string GetAppAbsolutePath(string relativePath);
        string GetServerPath(string relativePath);
    }

    public class PathProvider : IPathProvider
    {
        public string GetAppBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;

            return string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);
        }

        public string ConvertToDirectImageUrl(string url)
        {
            return GetAppBaseUrl() + url;
        }

        public string GetAppAbsolutePath(string relativePath)
        {
            relativePath = AmendAppRelativePath(relativePath);
            return VirtualPathUtility.ToAbsolute(relativePath);
        }

        public string GetServerPath(string relativePath)
        {
            return HostingEnvironment.MapPath(GetAppAbsolutePath(relativePath));
        }

        private string AmendAppRelativePath(string filePath)
        {
            return filePath[0] == '~' ? filePath : '~' + filePath;
        }
    }

    public static class PathHelper
    {
        public static string GetAppBaseUrl()
        {
            var pathProvider = new PathProvider();
            return pathProvider.GetAppBaseUrl();
        }

        public static string ConvertToDirectImageUrl(string url)
        {
            var pathProvider = new PathProvider();
            return pathProvider.ConvertToDirectImageUrl(url);
        }
    }
}