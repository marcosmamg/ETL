using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ETL
{
    class QueryBuilder
    {
        //Method to get Data from SQL Queries configured in XML File
        public static List<DataTable> GetDataFromSQLFiles()
        {
            List<DataTable> queries = new List<DataTable>();
            try
            {
                //Extracting queries from sql files
                string[] files = Directory.GetFiles(Utilities.BaseDirectory() + "queries\\", "*.sql", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    //Obtaining Data from sql
                    var datatable = DBClient.getQueryResultset(File.ReadAllText(file));                    
                    queries.Add(datatable);
                }
                if (queries.Count() == 0)
                {
                    Utilities.Log("Query Builder, status: Queries returned no result", "error");
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