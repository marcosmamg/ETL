using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ETL
{
    static class QueryBuilder
    {
        private const string QUERIES_FOLDER = "Queries\\";
        private const string FILE_TYPE = "*.sql";
        private static List<string> Queries
        {
            get
            {                
                List<string> _queries = new List<string>();
                string queriesFolder = Path.Combine(Utilities.BaseDirectory(), QUERIES_FOLDER);

                if (!Directory.Exists(queriesFolder))
                {
                    Directory.CreateDirectory(queriesFolder);                    
                    Utilities.Logger("Query Builder status: There were not queries defined \n" +
                                     "A folder /Queries with sql files must exist", "error");
                }
                else
                {                    
                    string[] files = Directory.GetFiles(queriesFolder, FILE_TYPE, SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            _queries.Add(File.ReadAllText(file));
                        };
                    }
                    else
                    {
                        Utilities.Logger("Query Builder status: There were not queries defined", "error");
                    }                    
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