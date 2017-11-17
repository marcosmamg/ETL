using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ETL
{
    class QueryBuilder
    {
        private const string QUERIES_FOLDER = "queries\\";
        private const string FILE_TYPE = "*.sql";

        private static List<string> Queries { get; } = new List<string>();

        public static List<DataTable> GetData()
        {
            List<DataTable> data = new List<DataTable>();
            try
            {
                GetSQLQueries();
                foreach (var query in Queries)
                {
                    data.Add(DBClient.GetQueryResultset(query));
                }               
            }
            catch (Exception ex)
            {
                Utilities.Log("Query Builder, status:" + ex.Message.ToString() + ex.ToString(), "error");
                throw ex;
            }

            return data;
        }

        //Obtains SQL queries from files
        private static void GetSQLQueries()
        {
            try
            {
                string[] files = Directory.GetFiles(Utilities.BaseDirectory() + QUERIES_FOLDER, FILE_TYPE, SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    Queries.Add(File.ReadAllText(file));                    
                }              
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }       
    }
}