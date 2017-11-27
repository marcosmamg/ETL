using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ETL
{
    static class QueryBuilder
    {
        private const string QUERIES_FOLDER = "queries\\";
        private const string FILE_TYPE = "*.sql";
        private static List<string> Queries
        {
            get
            {
                //TODO: SINGLETON
                List<string> _queries = new List<string>();
                string[] files = Directory.GetFiles(Utilities.BaseDirectory() + QUERIES_FOLDER, FILE_TYPE, SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    _queries.Add(File.ReadAllText(file));
                }                    
                return _queries;
            }
        }

        public static List<DataTable> GetData()
        {
            List<DataTable> data = new List<DataTable>();        
            try
            {                
                foreach (var query in Queries)
                {
                    data.Add(ExecuteQuery(query));
                }               
            }
            catch (Exception ex)
            {
                Utilities.Logger("Query Builder, status:" + ex.ToString(), "error");                
            }

            return data;
        }

        private static DataTable ExecuteQuery(string query)
        {
            DataTable queryData = new DataTable();
            try
            {
                queryData = DBClient.GetQueryResultset(query);
            }
            catch (Exception ex)
            {
                Utilities.Logger("Query Builder, status:" + ex.ToString(), "error");
            }
            return queryData;
        }
        
               
    }
}