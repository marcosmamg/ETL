using System;
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
        public int Port { get; set; }

        //Initilizes FTPClient
        public FTPClient(string _userName, string _password, string _URL, int _port = 21)
        {
            UserName = _userName;
            Password = _password;
            URL = _URL;
            Port = _port;
        }
        // Method receives file to upload it to a given path in the FTP defined in the AppConfig File
        public int UploadFile(String Path, String Filename, FileStream File)
        {
            try
            {
                // Get the object used to communicate with the server.  
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(URL + Path + Filename);
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

                return 0;
            }
            catch (WebException e)
            {
                Utilities.Log(e.Message.ToString() + e.ToString(), "error");                
                return 1;
            }
            catch (Exception ex)
            {
                Utilities.Log(ex.Message.ToString() + ex.ToString(), "error");
                return 1;
            }
        }
    }
}
