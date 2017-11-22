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
        private List<string> Folders = new List<string>();
        FtpWebRequest request;
        //TODO: USE ONLY ONE CONNECTION
        public FTPClient(string _userName, string _password, string _host, int _port)
        {
            Username = _userName;
            Password = _password;
            Url = new UriBuilder("ftp", _host, _port);            
        }
        // Method receives file stream to upload it to a given path in the FTP
        public bool UploadFile(MemoryStream file, string path)
        {
            bool result = false;
            try
            {               
                UriBuilder fullPath = new UriBuilder(Url.Scheme, Url.Host, Url.Port, path);                                
                
                request = (FtpWebRequest)WebRequest.Create(fullPath.ToString());
                request.Method = WebRequestMethods.Ftp.UploadFile;                
                request.Credentials = new NetworkCredential(Username, Password);
                request.KeepAlive = false;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(file.ToArray(), 0, file.ToArray().Length);
                requestStream.Close();                
                                
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();                
                Utilities.Logger("Upload File Complete, status:" + response.StatusDescription);
                response.Close();
                requestStream.Close();
                result =  true;
            }            
            catch (Exception ex)
            {
                Utilities.Logger("FTP Client:" + ex.Message.ToString() + ex.ToString(), "error");
                throw ex;
            }
            return result;
        }
        //Method to create folders if they do not exist in the FTP
        //Receives List<DataTable> to extract the distinct paths from the sql query
        public void GenerateFolderTree(List<DataTable> data)
        {
            try
            {
                foreach (var query in data)
                {
                    List<string> filePaths = query.AsEnumerable()
                                .Select(row => row.Field<string>("Path"))
                                .Distinct()
                                .ToList();

                    foreach (string path in filePaths)
                    {
                        string[] folderArray = path.Split('/');
                        string folderName = "";
                        for (int i = 0; i < folderArray.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(folderArray[i]))
                            {
                                //folderName = string.IsNullOrEmpty(folderName) ?
                                //            folderArray[i] : folderName + "/" + folderArray[i] + "/";
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
                                    Folders.Add(folderName);
                                    request = (FtpWebRequest)WebRequest.Create(Url + folderName);
                                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                                    request.Credentials = new NetworkCredential(Username, Password);
                                    request.KeepAlive = false;
                                    //TODO: USING INSTEAD
                                    var resp = (FtpWebResponse)request.GetResponse();
                                    resp.Close();
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
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (!(response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable))
                    {
                        //Folder already exist
                        Utilities.Logger("FTP Client:" + ex.Message.ToString() + ex.ToString(), "error");
                        throw ex;
                    }                    
                }                
                
            }
        }
    }
}
