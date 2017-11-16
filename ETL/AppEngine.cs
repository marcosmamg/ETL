using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace ETL
{
    public class ApplicationEngine
    {
        //arguments
        private static string username = string.Empty;
        private static string password = string.Empty;
        private static string url = string.Empty;
        private static string port = string.Empty;
        private static bool includeHeaderCSV = false;
        const string PORT_CONSTANT = "21";

        static void Main(string[] args)
        {           
            try
            {
                //Validating arguments
                if (!ValidateArguments(args))
                {
                    Utilities.Log("Please verify all the required parameters were correctly provided.", "error");
                    Environment.Exit(1);
                }
                //TODO: arguments["user"] requiered, variable,

                //Initializing FTPCLient                
                FTPClient myFtp = new FTPClient(username, password, url, port = PORT_CONSTANT);

                Console.WriteLine("Reading SQL File(s) and getting Data");                
                List<DataTable> queries= QueryBuilder.GetData();                
                foreach (var query in queries)
                {
                    var FilePaths = (from row in query.AsEnumerable()
                                     select row.Field<string>("Path")
                                    ).Distinct().ToList();
                    foreach (var Path in FilePaths)
                    {
                        Console.WriteLine("Generating CSV");
                        //Filtering data by path column
                        var QueryFiltered = query.AsEnumerable().Where(row => row.Field<string>("Path") == Path);
                        //Generating CSV with filtered datatable
                        var file = CsvGenerator.GenerateCSV(QueryFiltered.CopyToDataTable(), includeHeaderCSV);
                        Console.WriteLine("Uploading to FTP");                        
                        //Uploading File to FTP, passing file stream and unique filename
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
                Console.ReadLine();
                //Environment.Exit(1);
            }
        }
        private static bool ValidateArguments(string[] args)
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
                            includeHeaderCSV = bool.Parse(splitted[1]);
                            break;
                    };                    
                }
                else
                {
                    Utilities.Log("Error in parameters, please verify", "error");
                    Console.ReadLine();
                    //Environment.Exit(0);
                }
            }
            return (username != null && password != null && url != null);
        }
    }
}
