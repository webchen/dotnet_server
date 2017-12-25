using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using dotnet_server.DB;
using dotnet_server.Helper;

namespace dotnet_server.Logic
{
    class Mysql
    {
        /// <summary>
        /// 直接拼成SQL语句（注意，此方法 一定 要自行过滤，否则会产生SQL注入的风险）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="table"></param>
        /// <param name="where"></param>
        /// <param name="group"></param>
        /// <param name="order"></param>
        /// <param name="start"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string select(string db , string table , string field , string where , string group , string order , int start , int offset)
        {
            string json_str = "";
            StringBuilder sql = new StringBuilder();
            sql.Append("select " + field);
            sql.Append(" from " + table);
            if(string.Empty == where.Trim()) where = " 1 = 1 ";
            sql.Append(" where " + where);
            if(string.Empty != group)
            {
                sql.Append(" group by " + group);
            }
            if(string.Empty != order.Trim()) 
            {
                sql.Append(" order by " + order);
            }
            
            
            DataSet ds = MysqlDB.getInstance(db).Query(sql.ToString());
            json_str = JsonHelper.SerializeObject(ds);
            return json_str;
        }

        public static string query(string db , string sql)
        {
            string json_str = "";
            if(string.Empty == db.Trim() || string.Empty == sql.Trim())
            {
                return json_str;
            }
            //json_str = JsonConvert.SerializeObject(MysqlDB.getInstance(db).Query(sql).Tables[0]);
            json_str = JsonHelper.ToJson(MysqlDB.getInstance(db).Query(sql).Tables[0]);
            return json_str;
        }
    }
}
