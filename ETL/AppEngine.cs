using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace ETL
{
    public class ApplicationEngine
    {
        //private const int DEFAULT_FTP_PORT = 21;        
        private static string username = null;
        private static string password = null;
        private static string host = null;
        private static int port = 21;
        private static bool hasCSVHeaders = false;

        static void Main(string[] args)
        {
            try
            {
                if (!Utilities.HasValidArguments(args, out username, out password, out host, out port, out hasCSVHeaders))
                {
                    throw new Exception("The required parameters were not provided: " +
                                    "(username, password, host)");
                }

                Console.WriteLine("Reading SQL File(s) and getting Data");
                List<DataTable> data = QueryBuilder.GetData();

                Console.WriteLine("Generating folder tree in FTP");
                FTPClient ftp = new FTPClient(username, password, host, port);
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
                        ftp.UploadFile(file, ftpPath);
                    }
                }
                Console.WriteLine("Process completed succesfully");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Utilities.Logger(ex.Message.ToString() + ex.ToString(), "error");
                Environment.Exit(1);
            }
        }
    }
}