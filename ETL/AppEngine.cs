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
                string FilePath = "";
                string FileName = "";
                string FTPUsername = "";
                string FTPPassword = "";
                string FTPURL = "";
                string FTPPort = "";
                bool HasCSVHeader = false;
                //Validate parameters
                if (args.Length == 5)
                {
                    FTPUsername = args[0];
                    FTPPassword = args[1];
                    FTPURL = args[2];
                    FTPPort = args[3];
                    HasCSVHeader = bool.Parse(args[4]);
                }
                else
                {
                    Utilities.Log("Invalid number of parameters to execute task", "error");
                    Environment.Exit(1);
                }

                Console.WriteLine("Reading SQL File(s) and getting Data");                
                var queries= QueryBuilder.GetDataFromSQLFiles();
                
                foreach (var query in queries)
                {
                    Console.WriteLine("Generating CSV");
                    FilePath = query.Rows[0]["path"].ToString();
                    FileName = query.Rows[0]["fileName"].ToString();
                    var file = CsvGenerator.GenerateCSV(query, FileName, HasCSVHeader);
                    if (file == null)
                    {
                        Utilities.Log("CSV Filed not generated" + FilePath + FileName, "error");
                    }
                    else
                    {
                        Console.WriteLine("Uploading to FTP");
                        FTPClient myFtp = new FTPClient(FTPUsername, FTPPassword, FTPURL, FTPPort);
                        if (!myFtp.UploadFile(FilePath, FileName, file))
                        {
                            Utilities.Log("File Upload failed" + FilePath + FileName, "error");
                        }
                    }
                }

                Utilities.Log("Process completed succesfully");
                Console.ReadLine();
                //Environment.Exit(0);
            }
            catch (Exception ex)
            {

                Utilities.Log(ex.Message.ToString() + ex.ToString(), "error");
                Environment.Exit(1);
            }
        }        
    }
}
