using System;
using System.IO;
using System.Net;
using System.Configuration;
using System.Data.SqlClient;

namespace ETL
{     

    public class ApplicationEngine
    {
        static void Main(string[] args)
        {
            SqlDataReader results;
            try
            {                                
                results = DBClient.getQueryResultset("select itemid, skucode from items");
                CsvGenerator.GenerateCSV(results, Utilities.BaseDirectory(), "Test.csv");
                FTPClient myFtp = new FTPClient(ConfigurationManager.AppSettings["ftpUsername"], ConfigurationManager.AppSettings["ftpPassword"], ConfigurationManager.AppSettings["ftpURL"]);
                FileStream file = new FileStream(Utilities.BaseDirectory() + "Test.CSV", FileMode.Open, FileAccess.Read);
                if (myFtp.UploadFile(ConfigurationManager.AppSettings["itemsPath"], "Test.CSV", file) > 0)
                {
                    Console.WriteLine("File Upload Successful");
                }
                else
                {
                    Console.WriteLine("File Upload failed");
                }
            }
            catch (WebException e)
            {
                Utilities.LogError(e.Message.ToString());                                
                Utilities.LogError(((FtpWebResponse)e.Response).StatusDescription);                
            }
            catch (Exception ex)
            {
                Utilities.LogError(ex.Message.ToString());               
            }
        }        
    }
}
