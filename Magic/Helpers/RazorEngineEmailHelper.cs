using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace Penna.Messaging.Web.Helpers
{
    public static class RazorEngineEmailHelper
    {
        public class RazorEngineHtmlHelper
        {
            public IEncodedString Raw(string rawString)
            {
                return new RawString(rawString);
            }
        }

        public class HtmlSupportTemplateBase<T> : TemplateBase<T>
        {
            public RazorEngineHtmlHelper Html = new RazorEngineHtmlHelper();
        }

        public static TemplateServiceConfiguration GetTemplateServiceConfig()
        {
            // This config is neccessary: 
            // a) stop active directory email problems in TestMessagesController, 
            // b) allow Html.Raw to be used in template
            return new TemplateServiceConfiguration
            {
                CachingProvider = new DefaultCachingProvider(t => { }),
                BaseTemplateType = typeof (HtmlSupportTemplateBase<>)
            };
        }
    }
}