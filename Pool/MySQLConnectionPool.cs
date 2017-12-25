using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using dotnet_server.Core;
using dotnet_server.Model;
using MySql.Data.Common;
using MySql.Data.MySqlClient;

namespace dotnet_server.Pool
{
    class MySQLConnectionPool : ConnectionPool
    {
        /// <summary>
        /// 会有多个库，每个库的链接，有个名称，用于索引和标识
        /// </summary>
        public string Name;
        /// <summary>
        /// 连接池
        /// </summary>
        public IList<MySqlConnection> PoolList = new List<MySqlConnection>();
        /// <summary>
        /// 连接池总数
        /// </summary>
        protected int PoolMax = 1;
        /// <summary>
        /// 连接池管理对象实例
        /// </summary>
        public static Object objlock = typeof(MySQLConnectionPool);
        /// <summary>
        /// 已经使用的连接对象
        /// </summary>
        public int Used = 0;
        /// <summary>
        /// 连接数据库的字符串
        /// </summary>
        public string ConnectString = "";

        private static MySQLConnectionPool myPool = null;//池管理对象

        public MySQLConnectionPool(string name)
        {
            this.Name = name;
            this.ConnectString = this.getConnStr(name);
            this.PoolMax = this.getMaxPool(name);
        }

        public static MySQLConnectionPool GetPool(string name)
        {
            lock (objlock)
            {
                if (myPool == null)
                {
                    myPool = new MySQLConnectionPool(name);
                }
                return myPool;
            }
        }

        public MySqlConnection getConnection()
        {
            lock (PoolList)
            {
                MySqlConnection conn = null;
                //可用连接数量大于0
                if (PoolList.Count > 0)
                {
                    conn = (MySqlConnection)PoolList[0];
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    PoolList.RemoveAt(0);
                    Used--;
                }
                else
                {
                    if(Used <= PoolMax)
                    {
                        try
                        {
                            conn = new MySqlConnection(this.ConnectString);
                            conn.Open();
                            PoolList.Add(conn);
                            Used++;
                        }
                        catch(Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
                return conn;
            }
        }

        /// <summary>
        /// 仅仅将资源放回连接池，不做真正的关闭
        /// </summary>
        /// <param name="con"></param>
        public void Close(MySqlConnection con)
        {
            lock (PoolList)
            {
                if (con != null)
                {
                    //将连接添加在连接池中
                    PoolList.Add(con);
                    Used--;
                }
            }
        }

        /// <summary>
        /// 真正的关闭所有的数据库连接，清空连接池
        /// </summary>
        public void realClose()
        {
            foreach(var i in PoolList)
            {
                i.Close();
                PoolList.Remove(i);
            }
        }

    }
}
