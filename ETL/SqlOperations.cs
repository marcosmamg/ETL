using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace ETL
{
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
            catch{ }
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
