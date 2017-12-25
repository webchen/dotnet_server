using System;
using System.Net.Sockets;
using System.Text;
using dotnet_server;
using dotnet_server.Core;
using System.Collections.Generic;
using System.Collections.Specialized;
using dotnet_server.Helper;
using dotnet_server.Pool;
using dotnet_server.DB;
using System.Data;
using dotnet_server.Logic;

namespace dotnet_server.Core
{
    public class Response
    {
        protected string header = "";
        protected string content = "";

        public string getHeader()
        {
            return this.header;
        }
        public string getContent()
        {
            return this.content;
        }
        public void setHeader(string code , string contentType)
        {
            int len = HttpServer.charEncoder.GetBytes(this.content).Length;
            this.header = "HTTP/1.1 " + code + "\r\n"
                    + "Server: " + HttpServer.WebServerTitle + "\r\n"
                    + "Content-Length: " + len.ToString() + "\r\n"
                    + "Connection: close\r\n"
                    + "Content-Type: " + contentType + "\r\n\r\n";
            //Log.Write(len);
        }
        public void setOKHeader(string contentType)
        {
            this.setHeader(HttpServer.http_status_message(200) , contentType);
        }
        public void setOKHtmlHeader()
        {
            this.setOKHeader("text/html");
        }

        public void setOKJsonHeader()
        {
            this.setOKHeader("application/json");
        }

        public void setNotFoundHeader()
        {
            this.setHeader(HttpServer.http_status_message(404), "text/html");
        }

        public void setContent(string content)
        {
            this.content = content;
        }

        public void okHtml(string content)
        {
            this.setContent(content);
            this.setOKHtmlHeader();
        }

        public void notImplemented(Request request)
        {
            string content = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>" + HttpServer.WebServerTitle + "</h2><div>501 - Method Not Implemented</div></body></html>";
            this.setContent(content);
            this.setHeader("501 Not Implemented", "text/html");
        }

        public void notFound(Request request)
        {
            string content = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>" + HttpServer.WebServerTitle + "</h2><div>404 - Not Found</div></body></html>";
            this.setContent(content);
            this.setNotFoundHeader();
        }

        public void processQuery(Request request)
        {
            NameValueCollection query = new NameValueCollection();
            UrlHelper.ParseUrlQuery(request.queryString, out query);
            string db = query["db"].ToString().ToLower().Trim();
            string sql = System.Web.HttpUtility.UrlDecode(query["query"].ToString().Trim());
            string json = "";
            switch (query["type"].ToString().ToLower())
            {
                default:
                case "mysql":
                    json = Logic.Mysql.query(db , sql);
                break;
            }

            this.setContent(json);
            this.setOKJsonHeader();

        }

    }
}