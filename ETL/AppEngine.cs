using System;
using System.IO;
using System.Net;
using System.Configuration;
namespace ETL
{     

    public class ApplicationEngine
    {
        static void Main(string[] args)
        {
            try
            {
                //SqlDataReader results;
                //results = DBClient.ExecuteQuery("select itemid, skucode from items");
                FTPClient myFtp = new FTPClient(ConfigurationManager.AppSettings["ftpUsername"], ConfigurationManager.AppSettings["ftpPassword"], ConfigurationManager.AppSettings["ftpURL"]);
                FileStream file = new FileStream("C:\\debug1214.txt", FileMode.Open, FileAccess.Read);
                if (myFtp.UploadFile(ConfigurationManager.AppSettings["itemsPath"], "debug1214.csv", file) > 0)
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
                DBClient.LogError(e.Message.ToString());                
                String status = ((FtpWebResponse)e.Response).StatusDescription;
                DBClient.LogError(status);                
            }
            catch (Exception ex)
            {
                DBClient.LogError(ex.Message.ToString());               
            }
        }        
    }
}
