using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using dotnet_server.Core;
using dotnet_server.Model;

namespace dotnet_server.Core
{
    class HttpServer : Server
    {
        private int timeout = 100;
        protected Socket serverSocket;
        public static SiteConfig site_config = new SiteConfig();

        public static string http_status_message(int code)
        {
            switch (code)
            {
                case 100:
                    return "100 Continue";
                case 101:
                    return "101 Switching Protocols";
                case 201:
                    return "201 Created";
                case 202:
                    return "202 Accepted";
                case 203:
                    return "203 Non-Authoritative Information";
                case 204:
                    return "204 No Content";
                case 205:
                    return "205 Reset Content";
                case 206:
                    return "206 Partial Content";
                case 207:
                    return "207 Multi-Status";
                case 208:
                    return "208 Already Reported";
                case 226:
                    return "226 IM Used";
                case 300:
                    return "300 Multiple Choices";
                case 301:
                    return "301 Moved Permanently";
                case 302:
                    return "302 Found";
                case 303:
                    return "303 See Other";
                case 304:
                    return "304 Not Modified";
                case 305:
                    return "305 Use Proxy";
                case 307:
                    return "307 Temporary Redirect";
                case 400:
                    return "400 Bad Request";
                case 401:
                    return "401 Unauthorized";
                case 402:
                    return "402 Payment Required";
                case 403:
                    return "403 Forbidden";
                case 404:
                    return "404 Not Found";
                case 405:
                    return "405 Method Not Allowed";
                case 406:
                    return "406 Not Acceptable";
                case 407:
                    return "407 Proxy Authentication Required";
                case 408:
                    return "408 Request Timeout";
                case 409:
                    return "409 Conflict";
                case 410:
                    return "410 Gone";
                case 411:
                    return "411 Length Required";
                case 412:
                    return "412 Precondition Failed";
                case 413:
                    return "413 Request Entity Too Large";
                case 414:
                    return "414 Request URI Too Long";
                case 415:
                    return "415 Unsupported Media Type";
                case 416:
                    return "416 Requested Range Not Satisfiable";
                case 417:
                    return "417 Expectation Failed";
                case 418:
                    return "418 I'm a teapot";
                case 421:
                    return "421 Misdirected Request";
                case 422:
                    return "422 Unprocessable Entity";
                case 423:
                    return "423 Locked";
                case 424:
                    return "424 Failed Dependency";
                case 426:
                    return "426 Upgrade Required";
                case 428:
                    return "428 Precondition Required";
                case 429:
                    return "429 Too Many Requests";
                case 431:
                    return "431 Request Header Fields Too Large";
                case 500:
                    return "500 Internal Server Error";
                case 501:
                    return "501 Method Not Implemented";
                case 502:
                    return "502 Bad Gateway";
                case 503:
                    return "503 Service Unavailable";
                case 504:
                    return "504 Gateway Timeout";
                case 505:
                    return "505 HTTP Version Not Supported";
                case 506:
                    return "506 Variant Also Negotiates";
                case 507:
                    return "507 Insufficient Storage";
                case 508:
                    return "508 Loop Detected";
                case 510:
                    return "510 Not Extended";
                case 511:
                    return "511 Network Authentication Required";
                case 200:
                default:
                    return "200 OK";
            }
        }
        public static ServerException serverException = new ServerException();
        public const string WebServerTitle = "dotnet webserver";
        public static readonly Encoding charEncoder = Encoding.UTF8;
        public static Dictionary<string, string> Extensions = new Dictionary<string, string>()
        {
            { "htm", "text/html" },
            { "html", "text/html" },
            { "xml", "text/xml" },
            { "txt", "text/plain" },
            { "css", "text/css" },
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jpg", "image/jpg" },
            { "jpeg", "image/jpeg" },
            { "zip", "application/zip" },
            { "json", "application/json" }
        };
        public bool start()
        {
            site_config = Master.Conf["main"];
            int port = site_config.Port;
            if (this.check(port))
            {
                Log.Write("端口" + port + "已经被占用");
                return false;
            }
            return this.start(IPAddress.Any, port, 10);
        }

        /**
         * 检查端口是否被占用
         */
        protected bool check(int port)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    return true;
                }
            }
            return false;
        }

        protected bool start(IPAddress ipAddress, int port, int backLog)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(ipAddress, port));
                serverSocket.Listen(backLog);
                serverSocket.ReceiveTimeout = timeout;
                serverSocket.SendTimeout = timeout;
            }
            catch (Exception e)
            {
                Log.Write(e);
                return false;
            }

            // 多线程的方式运行，有请求到达，则启动一个新的线程
            Thread requestListenerT = new Thread(() =>
            {
                while (true)
                {
                    Socket clientSocket;
                    try
                    {
                        clientSocket = serverSocket.Accept();
                        // Create new thread to handle the request and continue to listen the socket.
                        Thread requestHandler = new Thread(() =>
                        {
                            Master.rq_times++;
                            Log.Write("请求次数：" + Master.rq_times.ToString());
                            //Log.Write(clientSocket.SocketType);     //Stream
                            //Log.Write(clientSocket.ReceiveBufferSize);      //8192
                            clientSocket.ReceiveTimeout = timeout;
                            clientSocket.SendTimeout = timeout;
                            try
                            {
                                var request = this.getRequest(clientSocket);
                                var response = new Response();
                                response = request.handle();
                                clientSocket.Send(HttpServer.charEncoder.GetBytes(response.getHeader()));
                                int sendsize = clientSocket.Send(HttpServer.charEncoder.GetBytes(response.getContent()));
                                clientSocket.Close();
                                //Log.Write(sendsize);
                            }
                            catch (Exception ee)
                            {
                                // TODO:这里经常报  由于连接方在一段时间后没有正确答复或连接的主机没有反应，连接尝试失败  的错误
                                // curl请求不会，浏览器请求就有
                                Log.Write(ee);
                                try
                                {
                                    clientSocket.Close();
                                } catch (Exception e) {
                                    Log.Write(e);
                                }
                            }
                        });
                        requestHandler.Start();
                    }
                    catch (Exception e)
                    {
                        Log.Write(e);
                    }
                }
            });
            requestListenerT.Start();

            return true;
        }

        protected void stop()
        {
            serverSocket.Close();
            serverSocket = null;
        }

        /*
        * 解析clientSocket，将需要的参数绑定到request对象上
        */
        protected Request getRequest(Socket clientSocket)
        {
            var request = new Request();
            byte[] buffer = new byte[10240];    // 10 kb
            int receivedCount = clientSocket.Receive(buffer);
            request.content = HttpServer.charEncoder.GetString(buffer, 0, receivedCount);
            // 客户端IP
            request.clientIp = (IPEndPoint)clientSocket.RemoteEndPoint;
            // 请求方法
            request.method = request.content.Substring(0, request.content.IndexOf(" "));

            int start = request.content.IndexOf(request.method) + request.method.Length + 1;
            int length = request.content.LastIndexOf("HTTP") - start - 1;
            request.requestUrl = request.content.Substring(start, length);
            
            if (request.method.Equals("GET") || request.method.Equals("POST"))
            {
                request.requestFile = request.requestUrl.Split('?')[0];
            }
            request.queryString = request.requestUrl.Replace(request.requestFile, "");
            //Host: (.*)?
            MatchCollection mc = Regex.Matches(request.content, @"Host: (.*)?", RegexOptions.IgnoreCase);
            if(mc.Count > 0 && mc[0].Success)
            {
                request.serverHost = mc[0].Groups[1].ToString().Trim().Split(":")[0];
                request.serverPort = int.Parse(mc[0].Groups[1].ToString().Trim().Split(":")[1]);
            }
            else
            {
                request.serverPort = 80;
            }

            return request;
        }

    }
}
