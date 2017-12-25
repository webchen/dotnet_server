using System;
using System.Collections.Generic;
using System.Text;
using dotnet_server.Core;
using dotnet_server.Model;

namespace dotnet_server
{
    sealed class Master
    {
        // 两层json
        public static readonly Dictionary<string, SiteConfig> Conf = Config.LoadSiteConfig("./Config/site.json");
        public static int rq_times = 0;

        public static void Main(string[] args)
        {
            Log.Write("start");
            var server = new Core.HttpServer();
            var ret = server.start();
            Log.Write(ret);
        }

    }
}
