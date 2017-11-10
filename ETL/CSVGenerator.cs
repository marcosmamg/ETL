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
        public static MemoryStream GenerateCSV(DataTable DatafromSQl, string FileName)
        {
            var memoryStream = new MemoryStream();
            try
            {
                //Creating a file                
                StreamWriter streamWriter = new StreamWriter(memoryStream);
                CsvWriter csv = new CsvWriter(streamWriter);                
                //Configuration of CSVWriter
                csv.Configuration.QuoteNoFields = true;
                csv.Configuration.Comment = '#';                    
                csv.Configuration.SanitizeForInjection = false;
                csv.Configuration.HasHeaderRecord = bool.Parse(ConfigurationManager.AppSettings["HasCSVHeader"]);

                if (csv.Configuration.HasHeaderRecord)
                {
                    foreach (DataColumn column in DatafromSQl.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();
                }                  

                // Write row values to File
                foreach (DataRow row in DatafromSQl.Rows)
                {
                    for (var i = 0; i < DatafromSQl.Columns.Count; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
                streamWriter.Flush();
                memoryStream.Position = 0;
                return memoryStream;
                
            }
            catch (Exception ex)
            {
                Utilities.Log("Generating CSV File:" + ex.Message + ex.ToString(), "error");
                return memoryStream;
            }
        }
    }


}
