using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using dotnet_server.Model;
using dotnet_server.Core;

namespace dotnet_server.Pool
{
    abstract class ConnectionPool
    {
        private static Dictionary<string, DBModel> conf;

        public ConnectionPool()
        {
            load_config();
        }

        private static void load_config()
        {
            if(conf == null) conf = Config.Loadconfig<Dictionary<string, DBModel>>("./Config/db.json");
        }

        /// <summary>
        /// 根据名称，获取相应的数据库连接字符串
        /// </summary>
        /// <returns></returns>
        protected string getConnStr(string name)
        {            
            return this.GetDB(name).ConnStr;
        }

        protected int getMaxPool(string name)
        {
            return this.GetDB(name).MaxPool;
        }

        /// <summary>
        /// 根据名称，获取对应的连接数据库的对象
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns></returns>
        protected DBModel GetDB(string name)
        {
            if(string.Empty == name || conf[name] == null)
            {
                string msg = "数据库配置文件不存在";
                Log.Error(msg);
                System.Threading.Thread.CurrentThread.Abort();
            }
            return conf[name];
        }

    }
}
