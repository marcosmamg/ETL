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
                
                //TODO: Create Path Folder for File if does not exist (levels?)
                CreateFolder(FullPath, Path);

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

                //Deleting Directory from File System                
                Utilities.RemoveFromFileSystem(Utilities.BaseDirectory() + Path + FileName, "file");
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
        private void CreateFolder(string FullPath, string RelativePath)
        {
            string[] folderArray = RelativePath.Split('/');
            string folderName = "";
            try
            {                
                for (int i = 0; i < folderArray.Length; i++)
                {
                    if (!string.IsNullOrEmpty(folderArray[i]))
                    {

                        folderName = string.IsNullOrEmpty(folderName) ? folderArray[i] : folderName + "/" + folderArray[i] + "/";
                        if (!CheckIfFolderExists(URL + ":" + Port + "/" + folderName))
                        {
                            //Create Folder
                            WebRequest requestRootFolder = WebRequest.Create(URL + ":" + Port + "/" + folderName);
                            requestRootFolder.Method = WebRequestMethods.Ftp.MakeDirectory;
                            requestRootFolder.Credentials = new NetworkCredential(UserName, Password);
                            using (var resp = (FtpWebResponse)requestRootFolder.GetResponse())
                            {
                                Console.WriteLine(resp.StatusCode);
                                Utilities.Log("FTP Client:" + resp.StatusCode);
                            }
                        }
                    }
                }                
            }
            catch (WebException ex)
            {
                Utilities.Log("FTP Client:" + ((FtpWebResponse)ex.Response).StatusDescription);                               
            }
        }

        private bool CheckIfFolderExists(string Path)
        {
            try
            {
                //Request object to list folder
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Path);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(UserName, Password);
                //Verifying if the folder was correctly listed
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {                    
                    return true;
                }
            }
            catch
            {
                return false;
            }
            
        }        
    }
}
