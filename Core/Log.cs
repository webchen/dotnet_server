using System;
using System.IO;

namespace dotnet_server.Core
{
    public static class Log
    {
        /*
        public enum Level
        {
            Debug = 1,
            Notice,
            Warning,
            Error,
        };
        */
        /*
        public static void setDir(string dir)
        {
            
        }
        */

        public static void Write(string log)
        {
            Console.WriteLine(log + "--------" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public static void Write(object log)
        {
            Write(log.ToString());
        }

        /// <summary>
        /// ���ݼ�����д��־����ͳһConsole.log�������������ƣ�
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level"></param>
        public static void Write(object log , int level)
        {
            Write(log);
        }

        public static void Error(object log)
        {
            Write(log);
        }

        public static void Notice(object log)
        {
            Write(log);
        }

    }
}