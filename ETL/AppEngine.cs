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
                //Getting arguments and saving
                var arguments = new Dictionary<string, string>();
                foreach (string argument in args)
                {
                    string[] splitted = argument.Split('=');

                    if (splitted.Length == 2)
                    {
                        arguments[splitted[0]] = splitted[1];
                    }
                }          
                Console.WriteLine("Reading SQL File(s) and getting Data");                
                List<DataTable> queries= QueryBuilder.GetDataFromSQLFiles();                
                foreach (var query in queries)
                {
                    var FilePaths = (from row in query.AsEnumerable()
                                     select row.Field<string>("Path")
                                    ).Distinct().ToList();
                    foreach (var Path in FilePaths)
                    {
                        Console.WriteLine("Generating CSV");
                        //Filtering query by path column
                        var QueryFiltered = query.AsEnumerable().Where(row => row.Field<string>("Path") == Path);
                        //Generating CSV with filtered datatable
                        var file = CsvGenerator.GenerateCSV(QueryFiltered.CopyToDataTable(), bool.Parse(arguments["includeheader"]));
                        Console.WriteLine("Uploading to FTP");
                        FTPClient myFtp = new FTPClient(arguments["user"], arguments["password"], arguments["url"], arguments["port"]);
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
                Environment.Exit(1);
            }
        }        
    }
}
