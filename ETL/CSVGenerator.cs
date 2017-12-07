using CsvHelper;
using System.IO;
using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;

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
                            //TODO: REPLACE COMMAS
                            csv.WriteField(row[i]);
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
    class SqlOperations
    {
        public string Operation { get; set; }
        public List<string> Params { get; set; }
        public string ColumnToReplace { get; set; }
        public string DefaultValue { get; set; }

        public List<SqlOperations> ExtractOperations(DataRow row)
        {
            List<SqlOperations> sqlOperations = new List<SqlOperations>();
            try
            {
                sqlOperations = JsonConvert.DeserializeObject<List<SqlOperations>>(row["Expressions"].ToString());
            }
            catch (Exception ex){}
            return sqlOperations;
        }

        public DataRow[] PerformOperations(SqlOperations sqlOperations, DataRow[] dataRows)
        {
            switch (sqlOperations.Operation)
            {
                case "isnull":
                    foreach (DataRow row in dataRows)
                    {
                        try
                        {
                            row[sqlOperations.ColumnToReplace] = row[sqlOperations.Params[0]] ?? row[sqlOperations.Params[1]];
                        }
                        catch
                        {
                            row[sqlOperations.ColumnToReplace] = sqlOperations.DefaultValue;
                        }                       
                    }
                    break;
                case "between":
                    foreach (DataRow row in dataRows)
                    {
                        try
                        {
                            if (DateTime.TryParse(row[sqlOperations.Params[0]].ToString(), out DateTime startingDate)
                                && DateTime.TryParse(row[sqlOperations.Params[1]].ToString(), out DateTime EndingDate))
                            {                                
                                row[sqlOperations.ColumnToReplace] = DateTime.Today >= startingDate
                                                                    && DateTime.Today <= EndingDate ?
                                                                    row[sqlOperations.Params[2]] : sqlOperations.DefaultValue;
                            }
                            else
                            {
                                row[sqlOperations.ColumnToReplace] = sqlOperations.DefaultValue;
                            }
                        }
                        catch
                        {
                            row[sqlOperations.ColumnToReplace] = sqlOperations.DefaultValue;
                        }                        
                    }
                    break;
            };
            return dataRows;
        }
    }
}