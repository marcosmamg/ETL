using System;
using System.IO;
using System.Configuration;
using System.Data;
using System.Collections.Generic;

namespace ETL
{     

    public class ApplicationEngine
    {
        static void Main(string[] args)
        {
            DataTable sqlResults;            
            try
            {
                Console.WriteLine("Reading XML");
                var queries= QueryBuilder.BuildQueries();                
                Console.WriteLine("Executing queries");
                
                sqlResults = DBClient.getQueryResultset("select itemid, skucode from items");
                if (sqlResults.Rows.Count > 0)
                {
                    Console.WriteLine("Generating CSV");
                    if (!CsvGenerator.GenerateCSV(sqlResults, "Test.csv"))
                    {
                        Utilities.Log("CSV Filed not generated", "error");                    
                    }
                    Console.WriteLine("Uploading to FTP");
                    FTPClient myFtp = new FTPClient(ConfigurationManager.AppSettings["ftpUsername"], ConfigurationManager.AppSettings["ftpPassword"], ConfigurationManager.AppSettings["ftpURL"]);
                    FileStream file = new FileStream(Utilities.BaseDirectory() + "Test.CSV", FileMode.Open, FileAccess.Read);
                    if (!myFtp.UploadFile("/public_html/ETL/items/", "Test.CSV", file))
                    {
                        Utilities.Log("File Upload failed", "error");                    
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
