using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using dotnet_server.Core;
using dotnet_server.Pool;
using MySql.Data;
using MySql.Data.Common;
using MySql.Data.MySqlClient;

namespace dotnet_server.DB
{
    sealed class MysqlDB : DB
    {
        /// <summary>
        /// 单例模式
        /// </summary>
        private static Dictionary<string , MysqlDB> _instance = new Dictionary<string, MysqlDB>();
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        private MySQLConnectionPool pconn = null;

        private MysqlDB(string key)
        {
            
        }

        public static MysqlDB getInstance(string key)
        {
            if (!_instance.ContainsKey(key))
            {
                _instance.Add(key , new MysqlDB(key));
                _instance[key].pconn = MySQLConnectionPool.GetPool(key);
            }
            return _instance[key];
        }

        //public MySqlConnection conn = MySQLConnectionPool.GetPool("wx").getConnection();

        /// <summary>
        /// 执行SQL语句，返回受影响的行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql)
        {
            MySqlConnection conn = pconn.getConnection();
            try
            {
                Log.Write("MysqlConnection Opened.");
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                return cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Log.Write("MySqlException Error:" + ex.ToString());
                if (Regex.IsMatch(ex.ToString(), ""))
                {
                    Log.Write(ex.Message);
                }
            }
            finally
            {
                pconn.Close(conn);
            }
            return -1;
        }

        /// <summary>
        /// 运行SQL语句，返回dataset
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="paras">参数</param>
        /// <returns>Dataset数据库集</returns>
        public DataSet Query(string sql, params MySqlParameter[] paras)
        {
            MySqlConnection conn = pconn.getConnection();
            MySqlDataAdapter sqlda = new MySqlDataAdapter(sql, conn);
            sqlda.SelectCommand.Parameters.AddRange(paras);
            DataSet ds = new DataSet();
            sqlda.Fill(ds);
            pconn.Close(conn);
            return ds;
        }

        /// <summary>
        /// 运行SQL语句，返回dataset。可一次执行多个查询。
        /// </summary>
        /// <param name="sql">sql语句（多个用 ; 分开）</param>
        /// <param name="paras">参数</param>
        /// <returns>Dataset数据库集</returns>
        public DataSet Query(string sql)
        {
            MySqlConnection conn = pconn.getConnection();
            MySqlDataAdapter sqlda = new MySqlDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            sqlda.Fill(ds);
            pconn.Close(conn);
            return ds;
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="sql">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object ExecuteScalar(string sql)
        {
            MySqlConnection conn = pconn.getConnection();
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                try
                {
                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (MySqlException e)
                {                    
                    Log.Error(e);
                }
                finally
                {
                    pconn.Close(conn);
                }
            }
            return null;
        }

        private void PrepareCommand(MySqlCommand cmd, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
        {
            MySqlConnection conn = pconn.getConnection();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
            pconn.Close(conn);
        }
    }
}
