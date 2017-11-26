using CsvHelper;
using System.IO;
using System;
using System.Data;
using System.Collections.Generic;

namespace ETL
{
    public class CsvGenerator
    {
        //Method that generates CSV File from SQLDatareader
        public static MemoryStream GenerateCSV(DataRow[] dataRows, DataColumnCollection columns, bool hasCSVHeader, string[] excludedColumns)
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
                    foreach (DataColumn column in columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();
                }
                
                foreach (DataRow row in dataRows)
                {
                    List<int> excludedColumnsIndex = new List<int>();
                    foreach (var column in excludedColumns)
                    {                        
                        excludedColumnsIndex.Add(row.Table.Columns.IndexOf(column));                            
                    }
                    
                    for (var i = 0; i < columns.Count ; i++)
                    {
                        if (excludedColumnsIndex.IndexOf(i) < 0)
                            csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }

                streamWriter.Flush();                
                memoryStream.Position = 0;                
            }
            catch (Exception ex)
            {
                Utilities.Logger("Generating CSV File:" + ex.ToString(), "error");
                throw ex;
            }
            return memoryStream;
        }
    }
}