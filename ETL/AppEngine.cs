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
                String FilePath = "";
                String FileName = "";
                //Validate parameters
                if (args.Length > 0)
                {
                    Utilities.Log("This console application does not accept parameters, avoid using them", "error");                    
                }

                Console.WriteLine("Reading XML");
                var queries= QueryBuilder.GetDataFromSQL();                
                Console.WriteLine("Executing queries");               
                
                if (queries.Count > 0)
                {
                    Console.WriteLine("Generating CSV");                    
                    foreach (var query in queries)
                    {                        
                        FilePath  = query.ExtendedProperties["Path"].ToString();
                        FileName = query.ExtendedProperties["FileName"].ToString();
                        if (!CsvGenerator.GenerateCSV(query, Utilities.BaseDirectory() + FilePath, FileName))
                        {
                            Utilities.Log("CSV Filed not generated" + FilePath + FileName, "error");
                        }
                        Console.WriteLine("Uploading to FTP");
                        FTPClient myFtp = new FTPClient(ConfigurationManager.AppSettings["ftpUsername"], ConfigurationManager.AppSettings["ftpPassword"], ConfigurationManager.AppSettings["ftpURL"], ConfigurationManager.AppSettings["ftpPort"]);
                        FileStream file = new FileStream(Utilities.BaseDirectory() + FilePath + FileName, FileMode.Open, FileAccess.Read);
                        if (!myFtp.UploadFile(FilePath, FileName, file))
                        {
                            Utilities.Log("File Upload failed" + FilePath + FileName, "error");
                        }
                    }                    
                }
                else
                {
                    Utilities.Log("SQL Queries returned no results", "error");
                }
                
                //Deleting tree of files
                Utilities.RemoveFromFileSystem(Utilities.BaseDirectory() + FilePath.Split('/')[1], "directory");

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
