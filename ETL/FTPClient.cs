using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
namespace ETL
{
    public class FTPClient
    {
        private string Username { get; set; }
        private string Password { get; set; }
        private UriBuilder Url { get; set; }
        public FtpWebRequest Request { get; set; }
        public FTPClient(string _userName, string _password, string _host, int _port)
        {
            Username = _userName;
            Password = _password;
            Url = new UriBuilder("ftp", _host, _port);
        }
        private void InitializeRequest(string method, string fullPath)
        {
            Request = (FtpWebRequest)WebRequest.Create(fullPath);
            Request.Method = method;
            Request.Credentials = new NetworkCredential(Username, Password);
            Request.KeepAlive = true;
        }
        // Method receives file stream to upload it to a given path in the FTP
        public bool UploadFile(MemoryStream file, string path)
        {
            bool result = false;
            try
            {
                UriBuilder fullPath = new UriBuilder(Url.Scheme, Url.Host, Url.Port, path);

                InitializeRequest(WebRequestMethods.Ftp.UploadFile, fullPath.ToString());

                Stream requestStream = Request.GetRequestStream();
                requestStream.Write(file.ToArray(), 0, file.ToArray().Length);
                requestStream.Close();

                using (var response = (FtpWebResponse)Request.GetResponse())
                {
                    Utilities.Logger("Upload File Complete, status:" + path + "\n" + response.StatusDescription);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Utilities.Logger("FTP Client:" + ex.ToString(), "error");
            }
            return result;
        }
        //Method to create folders if they do not exist in the FTP
        //Receives List<DataTable> to extract the distinct paths from the sql query
        public void GenerateFolderTree(List<DataTable> data)
        {
            try
            {
                List<string> Folders = new List<string>();
                foreach (var table in data)
                {
                    List<string> filePaths = table.AsEnumerable()
                                .Select(row => row.Field<string>("Path"))
                                .Distinct()
                                .ToList();

                    foreach (string filePath in filePaths)
                    {
                        string[] folderArray = filePath.Split('/');
                        string folderName = "";
                        for (int i = 0; i < folderArray.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(folderArray[i]))
                            {
                                if (string.IsNullOrEmpty(folderName))
                                {
                                    folderName = folderArray[i];
                                }
                                else
                                {
                                    folderName = folderName + "/" + folderArray[i] + "/";
                                }
                                if (Folders.IndexOf(folderName) == -1)
                                {
                                    CreateFolderInFTP(folderName);                                    
                                    Folders.Add(folderName);
                                                                        
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    Utilities.Logger("FTP Client:" + ex.ToString(), "error");
                    throw ex;
                }

            }
        }

        private void CreateFolderInFTP(string folderName)
        {            
            string fullPath = Path.Combine(Url.ToString() ,folderName);
            try
            {
                InitializeRequest(WebRequestMethods.Ftp.MakeDirectory, fullPath);

                using (var resp = (FtpWebResponse)Request.GetResponse()){}
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (!(response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable))
                    {
                        //There was an exception not related to folder exists
                        Utilities.Logger("FTP Client: Issue creating folder"
                                        + ex.Message.ToString() + ":" + fullPath
                                        , "error");
                        throw ex;
                    }                
                }
            }            
        }
    }
}