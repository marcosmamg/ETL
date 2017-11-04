using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ETL
{
    public class DBClient
    {
        private static ConnectionStringSettings strConnection = ConfigurationManager.ConnectionStrings["testETL"];
        private static void OpenConnection(ref SqlConnection objConnection)
        {
            try
            {
                objConnection = new SqlConnection();
                objConnection.ConnectionString = strConnection.ConnectionString; ;
                objConnection.Open();
            }
            catch (Exception Ex)
            {
                Utilities.Log("Opening Database Connection:" + Ex.Message, "error");
                Environment.Exit(1);
            }
        }       

        public static SqlDataReader getQueryResultset(string query)
        {
            SqlConnection sqlConnection = null;
            SqlCommand sqlQuery = null;
            SqlDataReader reader;
            DBClient.OpenConnection(ref sqlConnection);

            sqlQuery = new SqlCommand(query, sqlConnection);
            sqlQuery.CommandType = CommandType.Text;
            sqlQuery.CommandTimeout = 600;            
            reader = sqlQuery.ExecuteReader();
            sqlQuery.Dispose();
            return reader;
        }
    }
}
