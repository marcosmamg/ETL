using System.Data.SqlClient;
using CsvHelper;
using System.IO;
using System;
using System.Data;
using System.Configuration;

namespace ETL
{
    public class CsvGenerator
    {
        //Method that generates CSV File from SQLDatareader
        public static Boolean GenerateCSV(DataTable DatafromSQl, string Path, string FileName)
        {            
            try
            {
                //Create path if it does not exist
                System.IO.Directory.CreateDirectory(Path);
                //Creating a file
                using (var textWriter = File.CreateText(Path + FileName))
                using (var csv = new CsvWriter(textWriter))
                {
                    //Configuration of CSVWriter
                    csv.Configuration.QuoteNoFields = true;
                    csv.Configuration.Comment = '#';                    
                    csv.Configuration.SanitizeForInjection = false;
                    csv.Configuration.HasHeaderRecord = bool.Parse(ConfigurationManager.AppSettings["HasCSVHeader"]);

                    // Write columns to File
                    foreach (DataColumn column in DatafromSQl.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    // Write row values to File
                    foreach (DataRow row in DatafromSQl.Rows)
                    {
                        for (var i = 0; i < DatafromSQl.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Utilities.Log("Generating CSV File:" + ex.Message + ex.ToString(), "error");
                return false;
            }
        }
    }


}
