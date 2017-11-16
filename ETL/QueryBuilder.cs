using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ETL
{
    class QueryBuilder
    {
        private const string QUERIES_FOLDER = "queries\\";
        private const string FILE_TYPE = "*.sql";
        public static List<DataTable> GetData()
        {
            List<DataTable> data = new List<DataTable>();
            List<string> queries = new List<string>();
            try
            {
                queries = GetSQLQueries();
                foreach (var query in queries)
                {
                    data.Add(DBClient.GetQueryResultset(query));
                }
                return data;
            }
            catch (Exception ex)
            {
                Utilities.Log("Query Builder, status:" + ex.Message.ToString() + ex.ToString(), "error");
                return data;
            }
        }

        private static List<string> GetSQLQueries()
        {
            List<string> queries = new List<string>();
            try
            {
                //Extracting queries from sql files
                string[] files = Directory.GetFiles(Utilities.BaseDirectory() + QUERIES_FOLDER, FILE_TYPE, SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {                    
                    queries.Add(File.ReadAllText(file));                    
                }              
                return queries;                
            }
            catch (Exception ex)
            {
                Utilities.Log("Query Builder, status:" + ex.Message.ToString() + ex.ToString(), "error");
                return queries;
            }
        }       
    }
}