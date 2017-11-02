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
}
