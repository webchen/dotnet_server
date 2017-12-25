using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dotnet_server.Model;
using Newtonsoft.Json;

namespace dotnet_server.Core
{
    public static class Config
    {

        public static Dictionary<string , SiteConfig> LoadSiteConfig(string configFile)
        {
            return Loadconfig<Dictionary<string, SiteConfig>>(configFile);
        }

        public static T Loadconfig<T>(string configFile) where T : class
        {
            if(string.Empty == configFile || !File.Exists(configFile))
            {
                Log.Write("配置文件不存在");
                System.Threading.Thread.CurrentThread.Abort("配置文件不存在");
                return null;
            }
            string json = File.ReadAllText(configFile);
            return JsonConvert.DeserializeObject<T>(json);
        }

    }
}
