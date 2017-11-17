using CsvHelper;
using System.IO;
using System;
using System.Data;

namespace ETL
{
    public class CsvGenerator
    {
        //Method that generates CSV File from SQLDatareader
        public static MemoryStream GenerateCSV(DataTable data, bool hasCSVHeader)
        {
            var memoryStream = new MemoryStream();
            try
            {                             
                StreamWriter streamWriter = new StreamWriter(memoryStream);
                CsvWriter csv = new CsvWriter(streamWriter);                
                
                csv.Configuration.QuoteNoFields = true;
                csv.Configuration.Comment = '#';                    
                csv.Configuration.SanitizeForInjection = false;
                csv.Configuration.HasHeaderRecord = hasCSVHeader;

                if (csv.Configuration.HasHeaderRecord)
                {
                    foreach (DataColumn column in data.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();
                }
                
                foreach (DataRow row in data.Rows)
                {
                    for (var i = 0; i < data.Columns.Count; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }

                streamWriter.Flush();                
                memoryStream.Position = 0;                
            }
            catch (Exception ex)
            {
                Utilities.Log("Generating CSV File:" + ex.Message + ex.ToString(), "error");
                throw ex;
            }
            return memoryStream;
        }
    }
}