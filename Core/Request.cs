using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Net.Sockets;
using dotnet_server;
using dotnet_server.Core;
using System.Net.NetworkInformation;

namespace dotnet_server.Core
{
    public class Request
    {
        /// <summary>
        /// 整个 request 信息
        /// </summary>
        public string content = "";
        /// <summary>
        /// 请求的整个url（包括参数）
        /// </summary>
        public string requestUrl = "";
        /// <summary>
        /// 请求的文件名：如：index.html
        /// </summary>
        public string requestFile = "";
        /// <summary>
        /// 服务器上本地的文件路径
        /// </summary>
        public string baseFile = "";
        public Dictionary<string , string> RequestInfo = new Dictionary<string, string>();  // 用于保存收到的HTTP协议的内容，包括：user-agent，accept,accept-language等
        public string serverHost;
        public int serverPort;
        public DateTime requestTime = DateTime.Now;
        public IPEndPoint clientIp;
        public string method;
        public string queryString;
        public MatchCollection cookie;

        public Response handle()
        {
            var response = new Response();
            // 只处理get和post请求
            if (!this.method.Equals("GET") && !this.method.Equals("POST"))
            {
                response.notImplemented(this);
            }

            if(this.requestFile == "/favicon.ico") 
            {
                response.notFound(this);
                return response;
            }

            if(this.requestFile != "/")
            {
                string extension = this.requestFile.Substring(this.requestFile.LastIndexOf(".") + 1, (this.requestFile.Length - this.requestFile.LastIndexOf(".") - 1));
                this.baseFile = HttpServer.site_config.RootPath  + this.requestFile.Replace("/", Server.DS);
                if(File.Exists(this.baseFile))
                {
                    if (HttpServer.Extensions.ContainsKey(extension)) response.setOKHeader(HttpServer.Extensions[extension]);
                    else response.setOKHtmlHeader();
                    response.setContent(File.ReadAllText(this.baseFile));
                }
                else
                {
                    response.notFound(this);
                }
            }
            else
            {
                if(this.queryString == string.Empty)
                {
                    string defaultFile = HttpServer.site_config.RootPath + Server.DS + HttpServer.site_config.IndexFile;
                    if (File.Exists(defaultFile))
                    {
                        response.setOKHtmlHeader();
                        response.setContent(File.ReadAllText(defaultFile));
                    }
                    else
                    {
                        response.notFound(this);
                    }
                }
                else
                {
                    response.processQuery(this);
                }
            }

            return response;
        }

    }

}
