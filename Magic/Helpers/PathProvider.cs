using System.Web;
using System.Web.Hosting;

namespace Magic.Helpers
{
    public interface IPathProvider
    {
        string GetBaseUrl();
        string GetDirectImageUrl(string url);
        string GetAbsolutePath(string relativePath);
        string GetServerPath(string relativePath);
    }

    public class PathProvider : IPathProvider
    {
        public string GetBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;

            return string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);
        }

        public string GetDirectImageUrl(string url)
        {
            return GetBaseUrl() + url;
        }

        public string GetAbsolutePath(string relativePath)
        {
            return VirtualPathUtility.ToAbsolute(relativePath);
        }

        public string GetServerPath(string relativePath)
        {
            return HostingEnvironment.MapPath(GetAbsolutePath(relativePath));
        }
    }
}