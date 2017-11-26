using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace ETL
{
    public class ApplicationEngine
    {        
        private static string username = null;
        private static string password = null;
        private static string host = null;
        private static int ftpPort = 21;
        private static string rootFolder = null;
        private static bool hasCSVHeaders = false;

        static void Main(string[] args)
        {
            try
            {
                int tablesCount = 0;
                if (!HasValidArguments(args))
                {
                    throw new Exception("The required parameters were not provided: " +
                                    "(username, password, host)");
                }

                Console.WriteLine("Reading SQL File(s) and getting Data");
                List<DataTable> data = QueryBuilder.GetData();

                Console.WriteLine("Generating folder tree in FTP");
                FTPClient ftp = new FTPClient(username, password, host, ftpPort);
                ftp.GenerateFolderTree(data);                
                foreach (DataTable table in data)
                {
                    int filePathsCount = 0;                    
                    List<string> filePaths = table.AsEnumerable()
                                            .Select(row => row.Field<string>("Path"))
                                            .Distinct()
                                            .ToList();
                    
                    foreach (string path in filePaths)
                    {   
                        Console.WriteLine("Generating CSV");
                        //TODO:DATAROWS
                        string ftpPath = path + "/" + table.Rows[0]["FileName"].ToString();
                        DataRow[] csvData = table.Select("path='" + path + "'");
                        string[] excludedColumns = table.Rows[0]["Excludedcolumns"].ToString().Split(',');
                        System.IO.MemoryStream file = CsvGenerator.GenerateCSV(csvData, table.Columns, hasCSVHeaders, excludedColumns);

                        Console.WriteLine("Uploading to FTP");                        
                        //If Last file of datatables: Request KeepAlive = False to close connection
                        if (tablesCount == data.Count() && filePathsCount == filePaths.Count())
                        {
                            ftp.UploadFile(file, ftpPath, false);
                        }
                        else
                        {
                            ftp.UploadFile(file, ftpPath, true);
                        }
                        filePathsCount++;
                    }
                    tablesCount++;
                }                                
                Console.WriteLine("Process completed succesfully");
                Environment.Exit(0);                
            }
            catch (Exception ex)
            {
                Utilities.Logger(ex.ToString(), "error");
                Environment.Exit(1);
            }
        }

        public static bool HasValidArguments(string[] args)
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
                            case "host":
                                host = splitted[1];
                                break;
                            case "port":
                                ftpPort = int.Parse(splitted[1]);
                                break;
                            case "includeheader":
                                hasCSVHeaders = bool.Parse(splitted[1]);
                                break;
                        };
                    }
                    else
                    {
                        throw new Exception("Error in parameters, please verify");
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.Logger("Error with one or more parameters: \n" +
                                  "Number of parameters provided =" + args.Length +
                                  "\n Help: Port = (int number), includeheader = (true or false)",
                                  "error");
                foreach (string argument in args)
                {
                    Utilities.Logger(argument, "error");
                }
                throw ex;
            }
            return (username != null && password != null && host != null);
        }
    }
}