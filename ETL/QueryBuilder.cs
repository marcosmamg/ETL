using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ETL
{
    class QueryBuilder
    {
        //queries.xml file must be on the root of the .exe file
        private static XElement doc = XElement.Load(Utilities.BaseDirectory() + "queries.xml");
        
        //Method to get Data from SQL Queries configured in XML File
        public static List<DataTable> GetDataFromSQLFiles()
        {
            List<DataTable> queries = new List<DataTable>();
            try
            {
                //Extracting queries with no filters yet from XML
                string[] files = Directory.GetFiles(Utilities.BaseDirectory() + "queries\\", "*.sql", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
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
        private static bool IncludeColumnInFile(XElement element)
        {
            try
            {
                return bool.Parse(element.Attribute("includeInFile").Value.ToString());
            }
            catch
            {
                return false;
            }
            
        }
        private static String GetPath(DataTable datatable)
        {
            DataRow row = datatable.Rows[0];
            return row["path"].ToString();
        }
             

    }
}