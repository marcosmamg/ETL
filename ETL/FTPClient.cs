using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
namespace ETL
{
    public class FTPClient
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string URL { get; set; }
        public string Port { get; set; }

        //Initilizes FTPClient
        public FTPClient(string _userName, string _password, string _URL, string _port = "21")
        {
            UserName = _userName;
            Password = _password;
            URL = _URL;
            Port = _port;
        }
        // Method receives file to upload it to a given path in the FTP defined in the AppConfig File
        public Boolean UploadFile(string Path, string FileName, FileStream File)
        {
            try
            {
                //Build Full Path to upload file
                string FullPath = URL + ':' + Port + '/' + Path;
                
                //Create Root Folder if does not exist
                CreateFolder(URL + ':' + Port + ConfigurationManager.AppSettings["ftpRootPath"]);

                //Create Path Folder for File if does not exist
                CreateFolder(FullPath);

                // Get the object used to communicate with the server.                
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FullPath +  FileName);
                Console.WriteLine(URL + ':' + Port + Path + FileName);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Method = WebRequestMethods.Ftp.UploadFile;                
                request.Credentials = new NetworkCredential(UserName, Password);

                // Copy the contents of the file to the request stream.  
                StreamReader sourceStream = new StreamReader(File);
                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                sourceStream.Close();
                request.ContentLength = fileContents.Length;

                //Preparing stream to upload to FTP
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();
                
                //Uploading File
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();                
                Utilities.Log("Upload File Complete, status:" + response.StatusDescription);
                response.Close();

                //Deleting File from File System
                //RemoveFile(Utilities.BaseDirectory() + Path + FileName);
                return true;
            }
            catch (WebException e)
            {
                Utilities.Log("FTP Client:" + e.Message.ToString() + e.ToString(), "error");                
                return false;
            }
            catch (Exception ex)
            {
                Utilities.Log("FTP Client:" + ex.Message.ToString() + ex.ToString(), "error");
                return false;
            }
        }

        //Method to create folders if they do not exist
        private void CreateFolder(string FolderPath)
        {
            try
            {
                //Request object to list folder
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FolderPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(UserName, Password);
                
                //Verifying if the folder was correctly listed
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine("Root Folder already exist");
                }
            }
            catch (WebException ex)
            {
                //Exception was raised and the folder must be created
                if (ex.Response != null)
                {
                    //Request object to create folder if ActionNotTakenFileUnavailable is raised
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        //Create Folder
                        WebRequest requestRootFolder = WebRequest.Create(FolderPath);
                        requestRootFolder.Method = WebRequestMethods.Ftp.MakeDirectory;
                        requestRootFolder.Credentials = new NetworkCredential(UserName, Password);
                        using (var resp = (FtpWebResponse)requestRootFolder.GetResponse())
                        {
                            Console.WriteLine(resp.StatusCode);
                            Utilities.Log("FTP Client:" + resp.StatusCode);
                        }
                    }
                    else
                    {
                        Utilities.Log("FTP Client:" + ((FtpWebResponse)ex.Response).StatusDescription);
                    }
                    
                }
            }
        }
        //Method to delete files from File System
        private void RemoveFile(string FileName)
        {
            try
            {
                File.Delete(FileName);
            }
            catch (Exception ex)
            {
                Utilities.Log("FTP Client:" + ex.Message.ToString() + ex.ToString(), "error");                
            }

        }
    }
}
