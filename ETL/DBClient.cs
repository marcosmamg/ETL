using System;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;

namespace ETL
{
    public class DBClient
    {
        private static ConnectionStringSettings strConnection = ConfigurationManager.ConnectionStrings["testETL"];
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
                Utilities.Log("Opening Database Connection:" + ex.Message + ex.ToString(), "error");
                Environment.Exit(1);
            }
        }

        //Method that executes SQL statements, but not stored procedures
        public static DataTable getQueryResultset(string query)
        {
            OdbcConnection sqlConnection = null;
            DataSet dataset = new DataSet();
            //Opening DB Connection
            DBClient.OpenConnection(ref sqlConnection);
            using (sqlConnection)
            {
                //filling the DataSet.
                OdbcDataAdapter adapter = new OdbcDataAdapter(query, sqlConnection);
                adapter.Fill(dataset);
            }
            //Returning datatable
            return dataset.Tables[0];
        }
    }
}
