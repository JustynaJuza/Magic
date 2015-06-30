using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Web.Configuration;

namespace Magic.Helpers
{
    public class AppSettingsHelper
    {
        public static string Get(string key)
        {
            return Get<string>(key);
        }

        public static T Get<T>(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(appSetting)) throw new KeyNotFoundException(key);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(appSetting));
        }

        public static void Set<T>(string key, T value)
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            config.AppSettings.Settings[key].Value = value.ToString();
            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}