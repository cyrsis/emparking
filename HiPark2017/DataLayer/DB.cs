using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Db
    {
        public static string ConnectionString
        {
            get
            {
                string connStr= ConfigurationManager.ConnectionStrings["Carpark_ClientConnection"].ConnectionString;
                
                SqlConnectionStringBuilder sb= new SqlConnectionStringBuilder(connStr);

                sb.ApplicationName = ApplicationName ?? sb.ApplicationName;
                sb.ConnectTimeout = (ConnectionTimeout > 0) ? ConnectionTimeout : sb.ConnectTimeout;

                return sb.ToString();
            }
        }

        public static SqlConnection GetSqlConnection()
        {

             var conn = new SqlConnection(ConnectionString);
           
                
               conn.Open();

            return conn;
        }



        public static int ConnectionTimeout { get; set; }

        public static string ApplicationName
        {
            get; set;
        }

     
    }
}
