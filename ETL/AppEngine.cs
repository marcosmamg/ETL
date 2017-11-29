using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace ETL
{
    public class ApplicationEngine
    {
        private const int FIRST_ROW = 0;
        private static string username = null;
        private static string password = null;
        private static string host = null;
        private static int ftpPort = 21;
        private static bool hasCSVHeaders = false;
        static void Main(string[] args)
        {
            try
            {                
                if (!HasValidArguments(args))
                {
                    throw new Exception("The required parameters were not provided: " +
                                    "(username, password, host)");
                }

                Console.WriteLine("Reading SQL File(s) and getting Data");
                List<DataTable> data = QueryBuilder.GetData();

                if (data.Any())
                {
                    Console.WriteLine("Generating folder tree in FTP");
                    FTPClient ftp = new FTPClient(username, password, host, ftpPort);
                    ftp.GenerateFolderTree(data);

                    Console.WriteLine("Generating CSV and uploading file");
                    foreach (DataTable table in data)
                    {
                        IEnumerable<string> filePaths = table.AsEnumerable()
                                                        .Select(row => row.Field<string>("Path"))
                                                        .Distinct();

                        foreach (string path in filePaths)
                        {
                            DataRow[] csvData = table.Select("path='" + path + "'");
                            List<string> excludedColumns = table.Rows[FIRST_ROW]["Excludedcolumns"].ToString()
                                                           .Split(',')
                                                           .Select(s => s.Trim()).ToList();
                            System.IO.MemoryStream file = CsvGenerator.GenerateCSV(csvData, table.Columns, hasCSVHeaders, excludedColumns);

                            string fullPath = path + "/" + table.Rows[FIRST_ROW]["FileName"].ToString();
                            ftp.UploadFile(file, fullPath);
                        }
                    }

                    Console.WriteLine("Process completed succesfully");
                }                
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