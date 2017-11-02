using System.Data.SqlClient;
using CsvHelper;
using System.IO;

namespace ETL
{
    public class CsvGenerator
    {
        private static object csv;

        public static void GenerateCSV(SqlDataReader DatafromSQl, string Path, string fileName)
        {

            var hasHeaderBeenWritten = false;
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
        }
    }


}
