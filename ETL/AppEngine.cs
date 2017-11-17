using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace ETL
{
    public class ApplicationEngine
    {
        const string DEFAULT_FTP_PORT = "21";
        
        private static string username = null;
        private static string password = null;
        private static string url = null;
        private static string port = null;
        private static bool hasCSVHeaders = false;

        static void Main(string[] args)
        {           
            try
            {
                HasValidArguments(args);
                
                Console.WriteLine("Reading SQL File(s) and getting Data");                
                List<DataTable> data= QueryBuilder.GetData();
                
                Console.WriteLine("Generating folder tree in FTP");
                FTPClient ftp = new FTPClient(username, password, url, port ?? DEFAULT_FTP_PORT);
                ftp.GenerateFolderTree(data);

                foreach (DataTable table in data)
                {
                    List<string> filePaths = table.AsEnumerable()
                                            .Select(row => row.Field<string>("Path"))
                                            .Distinct()
                                            .ToList();

                    foreach (string path in filePaths)
                    {
                        Console.WriteLine("Generating CSV");
                        DataTable csvData = table.AsEnumerable()
                                            .Where(row => row.Field<string>("Path") == path)
                                            .CopyToDataTable();

                        System.IO.MemoryStream file = CsvGenerator.GenerateCSV(csvData, hasCSVHeaders);

                        Console.WriteLine("Uploading to FTP");
                        string ftpPath = path + "/" + table.Rows[0]["FileName"].ToString();
                        ftp.UploadFile(file,ftpPath);
                    }                    
                }
                Console.WriteLine("Process completed succesfully");                
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Utilities.Log(ex.Message.ToString() + ex.ToString(), "error");                
                Environment.Exit(1);
            }
        }
        private static bool HasValidArguments(string[] args)
        {
            try
            {                
                foreach (string argument in args)
                {
                    string[] splitted = argument.Split('=');

                    if (splitted.Length == 2)
                    {
                        switch (splitted[0])
                        {
                            case "user":
                                username = splitted[1];
                                break;
                            case "password":
                                password = splitted[1];
                                break;
                            case "url":
                                url = splitted[1];
                                break;
                            case "port":
                                port = splitted[1];
                                break;
                            case "includeheader":
                                hasCSVHeaders = bool.Parse(splitted[1]);
                                break;
                        };
                    }
                    else
                    {
                        Utilities.Log("Error in parameters, please verify", "error");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Number of command line parameters = {0}", args.Length);

                foreach (string s in args)
                {
                    System.Console.WriteLine(s);
                }
                throw ex;
            }
            return (username != null && password != null && url != null);
        }
    }
}
