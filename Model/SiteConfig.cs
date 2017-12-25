using System;
using System.Collections.Generic;
using System.Text;

namespace dotnet_server.Model
{
    public class SiteConfig
    {
        public string Domain { get; set; }
        public int Port { get; set; }
        public string RootPath { get; set; }
        public string IndexFile { get; set; }
    }

}
