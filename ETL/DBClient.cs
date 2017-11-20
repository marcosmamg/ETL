using System;
using System.Configuration;
using System.Data;
using System.Data.Odbc;

namespace ETL
{
    public static class DBClient
    {
        private static ConnectionStringSettings strConnection = ConfigurationManager.ConnectionStrings["ETL"];
        // Method to open DB Connection
        private static void OpenConnection(ref OdbcConnection objConnection)
        {
            try
            {
                objConnection = new OdbcConnection(strConnection.ConnectionString);
                objConnection.Open();
            }
            catch (Exception ex)
            {
                Utilities.Logger("Opening Database Connection:" + ex.Message + ex.ToString(), "error");
                throw ex;
            }
        }
        //Method that executes SQL statements
        public static DataTable GetQueryResultset(string query)
        {
            OdbcConnection sqlConnection = null;
            DataSet dataset = new DataSet();
            try
            {                
                DBClient.OpenConnection(ref sqlConnection);
                using (sqlConnection)
                {
                    OdbcDataAdapter adapter = new OdbcDataAdapter(query, sqlConnection);
                    adapter.Fill(dataset);
                }
            }
            catch (Exception ex)
            {
                Utilities.Logger("Executing SQL" + ex.Message + ex.ToString(), "error");
                throw ex;
            }
            return dataset.Tables[0];
        }
    }
}