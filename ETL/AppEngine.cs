using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ETL
{     

    public class ApplicationEngine
    {
        static void Main(string[] args)
        {            
            try
            {
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
                List<DataTable> queries= QueryBuilder.GetDataFromSQLFiles();                
                foreach (var query in queries)
                {
                    var FilePaths = (from row in query.AsEnumerable()
                                     select row.Field<string>("Path")).Distinct().ToList();
                    foreach (var Path in FilePaths)
                    {
                        Console.WriteLine("Generating CSV");
                        var QueryFiltered = query.AsEnumerable().Where(row => row.Field<string>("Path") == Path);
                        var file = CsvGenerator.GenerateCSV(QueryFiltered.CopyToDataTable(), HasCSVHeader);
                        Console.WriteLine("Uploading to FTP");
                        FTPClient myFtp = new FTPClient(FTPUsername, FTPPassword, FTPURL, FTPPort);
                        myFtp.UploadFile(file, Path, query.Rows[0]["FileName"].ToString());
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
