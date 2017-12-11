using CsvHelper;
using System.IO;
using System;
using System.Data;
using System.Collections.Generic;

namespace ETL
{    
    static class CsvGenerator
    {
        //Method that generates CSV File from SQLDatareader
        public static MemoryStream GenerateCSV(DataRow[] dataRows, DataColumnCollection columns, bool hasCSVHeader, List<string> excludedColumns)
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
                        if (!excludedColumns.Contains(column.ColumnName))
                        {
                            csv.WriteField(column.ColumnName);
                        }
                    }
                    csv.NextRecord();
                }

                SqlOperations sqlOperations = new SqlOperations();
                List<SqlOperations> operations = sqlOperations.ExtractOperations(dataRows[0]);
                foreach(var operation in operations)
                {
                    dataRows = sqlOperations.PerformOperations(operation, dataRows);                    
                }
                
                foreach (DataRow row in dataRows)
                {
                    for (var i = 0; i < columns.Count; i++)
                    {
                        if (!excludedColumns.Contains(columns[i].ColumnName))
                        {
                            string field = row[i].ToString().Replace(",", "");
                            csv.WriteField(field);
                        }
                            
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