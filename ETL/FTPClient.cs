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
        
        public FTPClient(string _userName, string _password, string _url, string _port)
        {
            try
            {
                Username = _userName;
                Password = _password;
                Url = new UriBuilder("ftp", _url, int.Parse(_port));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
        // Method receives file stream to upload it to a given path in the FTP
        public bool UploadFile(MemoryStream file, string path)
        {
            bool result = false;
            try
            {               
                UriBuilder fullPath = new UriBuilder(Url.Scheme, Url.Host, Url.Port, path);                                
                
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullPath.ToString());
                request.Method = WebRequestMethods.Ftp.UploadFile;                
                request.Credentials = new NetworkCredential(Username, Password);
                                
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(file.ToArray(), 0, file.ToArray().Length);
                requestStream.Close();                
                                
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();                
                Utilities.Log("Upload File Complete, status:" + response.StatusDescription);
                response.Close();
                result =  true;
            }            
            catch (Exception ex)
            {
                Utilities.Log("FTP Client:" + ex.Message.ToString() + ex.ToString(), "error");
                throw ex;
            }
            return result;
        }
        //Method to create folders if they do not exist in the FTP
        //Receives List<DataTable> to extract the distinct paths from the sql query
        public void GenerateFolderTree(List<DataTable> data)
        {
            string folderName = "";
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

                        for (int i = 0; i < folderArray.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(folderArray[i]))
                            {
                                folderName = string.IsNullOrEmpty(folderName) ?
                                            folderArray[i] : folderName + "/" + folderArray[i] + "/";

                                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Url + folderName);
                                request.Method = WebRequestMethods.Ftp.ListDirectory;
                                request.Credentials = new NetworkCredential(Username, Password);
                                //Verifying if the folder was correctly listed
                                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                                {                                        
                                }                                    
                                
                            }
                        }
                    }
                }                                
            }
            catch (WebException ex)
            {
                //Create Folder
                WebRequest requestRootFolder = WebRequest.Create(Url + folderName);
                requestRootFolder.Method = WebRequestMethods.Ftp.MakeDirectory;
                requestRootFolder.Credentials = new NetworkCredential(Username, Password);

                using (var resp = (FtpWebResponse)requestRootFolder.GetResponse())
                {                    
                    Utilities.Log("FTP Client:" + resp.StatusCode);
                }
            }
        }        
    }
}
