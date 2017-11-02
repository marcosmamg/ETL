using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
namespace ETL
{
    public class DBClient
    {
        private static string strConnection = ConfigurationManager.AppSettings["DSN"];
        public static void OpenConnection(ref SqlConnection objConn)
        {
            try
            {
                objConn = new SqlConnection();
                objConn.ConnectionString = strConnection;
                objConn.Open();
            }
            catch (Exception Ex)
            {
                LogError("Opening Database Connection:" + Ex.Message);
            }
        }
        public static void LogError(string strError)
        {
            System.Diagnostics.EventLog objEventLog = new System.Diagnostics.EventLog("Application");
            objEventLog.Source = "ETL App";
            objEventLog.WriteEntry(strError, System.Diagnostics.EventLogEntryType.Error, 1);
            objEventLog = null;
        }

        public static SqlDataReader ExecuteQuery(string query)
        {
            SqlConnection sqlConn = null;
            SqlCommand sqlComm = null;
            SqlDataReader reader;
            DBClient.OpenConnection(ref sqlConn);

            sqlComm = new SqlCommand(query, sqlConn);
            sqlComm.CommandType = CommandType.Text;
            sqlComm.CommandTimeout = 600;            
            reader = sqlComm.ExecuteReader();
            sqlComm.Dispose();
            return reader;
        }
    }
}
