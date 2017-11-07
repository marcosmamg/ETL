using System.Data.SqlClient;
using CsvHelper;
using System.IO;
using System;

namespace ETL
{
    public class CsvGenerator
    {
        //Method that generates CSV File from SQLDatareader
        public static int GenerateCSV(SqlDataReader DatafromSQl, string Path, string fileName)
        {
            try
            {
                var hasHeaderBeenWritten = false;
                
                //Creating a file
                using (var textWriter = File.CreateText(fileName))
                using (var csv = new CsvWriter(textWriter))
                {
                    //Iterates over SQLDataReader
                    while (DatafromSQl.Read())
                    {
                        //Validates if header has been writen
                        if (!hasHeaderBeenWritten)
                        {
                            for (var i = 0; i < DatafromSQl.FieldCount; i++)
                            {
                                csv.WriteField(DatafromSQl.GetName(i));
                            }
                            csv.NextRecord();
                            hasHeaderBeenWritten = true;
                        }

                        //Iterate over Data to write in File
                        for (var i = 0; i < DatafromSQl.FieldCount; i++)
                        {
                            csv.WriteField(DatafromSQl[i]);
                        }
                        csv.NextRecord();
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Utilities.Log("Generating CSV File:" + ex.Message + ex.ToString(), "error");
                return 1;
            }
        }
    }


}
