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
            try
            {
                Console.WriteLine("Reading XML");
                var queries= QueryBuilder.GetDataFromSQL();                
                Console.WriteLine("Executing queries");               
                
                if (queries.Count > 0)
                {
                    Console.WriteLine("Generating CSV");
                    var i = 0;
                    foreach (var query in queries)
                    {
                        i++;
                        String FilePath  = query.ExtendedProperties["Path"].ToString();
                        String FileName = query.ExtendedProperties["FileName"].ToString();
                        if (!CsvGenerator.GenerateCSV(query, i + "Test.csv"))
                        {
                            Utilities.Log("CSV Filed not generated", "error");
                        }
                        Console.WriteLine("Uploading to FTP");
                        FTPClient myFtp = new FTPClient(ConfigurationManager.AppSettings["ftpUsername"], ConfigurationManager.AppSettings["ftpPassword"], ConfigurationManager.AppSettings["ftpURL"]);
                        FileStream file = new FileStream(Utilities.BaseDirectory() + i +"Test.CSV", FileMode.Open, FileAccess.Read);
                        if (!myFtp.UploadFile(FilePath, i + "Test.CSV", file))
                        {
                            Utilities.Log("File Upload failed", "error");
                        }
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
