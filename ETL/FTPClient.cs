using System;
using System.IO;
using System.Net;
using System.Text;
namespace ETL
{
    public class FTPClient
    {
        private string UserName { get; set; }
        private string Password { get; set; }
        private string URL { get; set; }
        private string Port { get; set; }
        private const string PORT_CONSTANT = "21";
        //Initilizes FTPClient
        public FTPClient(string _userName, string _password, string _URL, string _port = PORT_CONSTANT)
        {
            UserName = _userName;
            Password = _password;
            URL = _URL;
            Port = _port;
        }
        // Method receives file stream to upload it to a given path in the FTP defined in the AppConfig File
        public Boolean UploadFile(MemoryStream File, string Path, string FileName)
        {
            try
            {
                //Build Full Path to upload file
                string FullPath = URL + ':' + Port + '/' + Path + '/';
                URL = URL + ":" + Port + "/";
                //Create Path Folder for File if does not exist
                CreateFolder(Path);
                // Get the object used to communicate with the server.     
                
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FullPath +  FileName);
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
        private void CreateFolder(string RelativePath)
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
                        if (!CheckIfFolderExists(URL + folderName))
                        {
                            //Create Folder
                            WebRequest requestRootFolder = WebRequest.Create(URL + folderName);
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
