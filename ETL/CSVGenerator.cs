using System.Data.SqlClient;
using CsvHelper;
using System.IO;
using System;

namespace ETL
{
    public class CsvGenerator
    {
        public static int GenerateCSV(SqlDataReader DatafromSQl, string Path, string fileName)
        {
            try
            {
                var hasHeaderBeenWritten = false;
                //Creating a file
                using (var textWriter = File.CreateText(fileName))
                using (var csv = new CsvWriter(textWriter))
                {
                    while (DatafromSQl.Read())
                    {
                        if (!hasHeaderBeenWritten)
                        {
                            for (var i = 0; i < DatafromSQl.FieldCount; i++)
                            {
                                csv.WriteField(DatafromSQl.GetName(i));
                            }
                            csv.NextRecord();
                            hasHeaderBeenWritten = true;
                        }
                        for (var i = 0; i < DatafromSQl.FieldCount; i++)
                        {
                            csv.WriteField(DatafromSQl[i]);
                        }
                        csv.NextRecord();
                    }
                }
                return 0;
            }
            catch (Exception Ex)
            {
                Utilities.Log("Generating CSV File:" + Ex.Message, "error");
                return 1;
            }
        }
    }


}
