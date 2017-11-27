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
        private void InitializeRequest(string method, string fullPath, bool keepAlive)
        {
            Request = (FtpWebRequest)WebRequest.Create(fullPath);
            Request.Method = method;
            Request.Credentials = new NetworkCredential(Username, Password);
            Request.KeepAlive = keepAlive;            
        }
        // Method receives file stream to upload it to a given path in the FTP
        public bool UploadFile(MemoryStream file, string path, bool keepAlive)
        {
            bool result = false;
            try
            {               
                UriBuilder fullPath = new UriBuilder(Url.Scheme, Url.Host, Url.Port, path);

                InitializeRequest(WebRequestMethods.Ftp.UploadFile, fullPath.ToString(), keepAlive);

                Stream requestStream = Request.GetRequestStream();
                requestStream.Write(file.ToArray(), 0, file.ToArray().Length);
                requestStream.Close();

                using (var response = (FtpWebResponse)Request.GetResponse())
                {
                    Utilities.Logger("Upload File Complete, status:" + path + "\n" +response.StatusDescription);
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
        public void GenerateFolderTree(List<string> paths)
        {            
            try
            {                
                foreach (string path in paths)
                {
                    string[] folderArray = path.Split('/');
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
                            if (!IsInLocalTree(folderName))
                            {
                                if (CreateFolderInFTP(folderName))
                                {                                        
                                    SaveFoldersTreeLocally(folderName);
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

        private bool IsInLocalTree(string folderName)
        {
            string[] treeFile = File.ReadAllLines(Utilities.BaseDirectory() + "/Logs/foldersTree.txt");
            List<string> treeList = new List<string>(treeFile);            
            return treeList.IndexOf(folderName) >= 0;
        }

        private void SaveFoldersTreeLocally(string folderName)
        {
            using (StreamWriter outputFile = new StreamWriter(Utilities.BaseDirectory() + "Logs/" + @"foldersTree.txt", true))
            {
                outputFile.WriteLine(folderName);
            }
        }

        private bool CreateFolderInFTP(string folderName)
        {
            bool result;
            try
            {
                InitializeRequest(WebRequestMethods.Ftp.MakeDirectory, Url + folderName, true);

                using (var resp = (FtpWebResponse)Request.GetResponse())
                {
                    result = true;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}