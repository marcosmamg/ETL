using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Configuration;
using System.Data;
using CsvHelper;
using System.Data.SqlClient;

namespace ETL
{
    public class DBClient
    {
        private static string strConnection = ConfigurationManager.AppSettings["DSN"];
        public static void OpenConnection(ref SqlConnection objConn)
        {
            try
            {
                objConn = new SqlConnection();
                objConn.ConnectionString = strConnection;
                objConn.Open();
            }
            catch (Exception Ex)
            {
                LogError("Opening Database Connection:" + Ex.Message);
            }
        }
        public static void LogError(string strError)
        {
            System.Diagnostics.EventLog objEventLog = new System.Diagnostics.EventLog("Application");
            objEventLog.Source = "ETL App";
            objEventLog.WriteEntry(strError, System.Diagnostics.EventLogEntryType.Error, 1);
            objEventLog = null;
        }

        public static SqlDataReader ExecuteQuery(string query)
        {
            SqlConnection sqlConn = null;
            SqlCommand sqlComm = null;
            SqlDataReader reader;
            DBClient.OpenConnection(ref sqlConn);

            sqlComm = new SqlCommand(query, sqlConn);
            sqlComm.CommandType = CommandType.Text;
            sqlComm.CommandTimeout = 600;
            //sqlComm.Parameters.AddWithValue("@District", strImportLine[15].Trim());
            reader = sqlComm.ExecuteReader();
            sqlComm.Dispose();
            return reader;
        }
    }
    public class FTPClient
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string URL { get; set; }
        public int Port { get; set; }

        public FTPClient(string _userName, string _password, string _URL, int _port = 21)
        {
            UserName = _userName;
            Password = _password;
            URL = _URL;
            Port = _port;
        }

        public int UploadFile(String Path, String Filename, FileStream File)
        {
            try
            {
                // Get the object used to communicate with the server.  
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(URL + Path + Filename);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // This example assumes the FTP site uses anonymous logon.  
                request.Credentials = new NetworkCredential(UserName, Password);

                // Copy the contents of the file to the request stream.  
                StreamReader sourceStream = new StreamReader(File);
                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                sourceStream.Close();
                request.ContentLength = fileContents.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                response.Close();

                return 1;
            }
            catch (WebException e)
            {
                DBClient.LogError(e.Message.ToString());
                //Console.WriteLine(e.Message.ToString());
                String status = ((FtpWebResponse)e.Response).StatusDescription;
                DBClient.LogError(status);
                return 0;
            }
            catch (Exception ex)
            {
                DBClient.LogError(ex.Message.ToString());
                return 0;
            }
        }
    }

    public class CsvGenerator
    {
        public void GenerateCSV(DataSet DatafromSQl, string Path)
        {
            
           
        }
    }

    public class ApplicationEngine
    {
        static void Main(string[] args)
        {
            SqlDataReader results;
            results = DBClient.ExecuteQuery("select itemid, skucode from items");
            FTPClient myFtp = new FTPClient(ConfigurationManager.AppSettings["ftpUsername"], ConfigurationManager.AppSettings["ftpPassword"], ConfigurationManager.AppSettings["ftpURL"]);
            FileStream fs = new FileStream("C:\\testfile.txt", FileMode.Open, FileAccess.Read);
            if (myFtp.UploadFile(ConfigurationManager.AppSettings["itemsPath"], "tesftile.csv", fs) > 0)
            {
                Console.WriteLine("File Upload Successful");
            }
            else
            {
                Console.WriteLine("File Upload failed");
            }
        }        
    }
}
