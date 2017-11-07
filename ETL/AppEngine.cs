using System;
using System.IO;
using System.Net;
using System.Configuration;
using System.Data.SqlClient;

namespace ETL
{     

    public class ApplicationEngine
    {
        static void Main(string[] args)
        {
            SqlDataReader sqlResults;            
            try
            {
                var a = QueryBuilder.ReadXML();
                foreach (var pair in a)
                {
                    Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
                }
                sqlResults = DBClient.getQueryResultset("select itemid, skucode from items");
                if (sqlResults.HasRows)
                {
                    if (CsvGenerator.GenerateCSV(sqlResults, Utilities.BaseDirectory(), "Test.csv") != 0)
                    {
                        Utilities.Log("CSV Filed not generated", "error");
                        Environment.Exit(1);
                    }
                    FTPClient myFtp = new FTPClient(ConfigurationManager.AppSettings["ftpUsername"], ConfigurationManager.AppSettings["ftpPassword"], ConfigurationManager.AppSettings["ftpURL"]);
                    FileStream file = new FileStream(Utilities.BaseDirectory() + "Test.CSV", FileMode.Open, FileAccess.Read);
                    if (myFtp.UploadFile(ConfigurationManager.AppSettings["itemsPath"], "Test.CSV", file) != 0)
                    {
                        Utilities.Log("File xxx Upload failed", "error");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    Utilities.Log("SQL Query returned no results", "error");                    
                }
                Utilities.Log("Process completed succesfully");
                Environment.Exit(0);
            }            
            catch (Exception ex)
            {
                Utilities.Log(ex.Message.ToString() + ex.ToString(), "error");
                Environment.Exit(1);
            }
        }        
    }
}
