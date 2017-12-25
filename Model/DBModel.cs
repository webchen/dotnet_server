using System;
using System.Collections.Generic;
using System.Text;

namespace dotnet_server.Model
{
    public class DBModel
    {
        public string Type { get; set; }
        public string ConnStr { get; set; }
        public int MaxPool { get; set; }
        public bool Debug { get; set; }
    }

}
