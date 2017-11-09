﻿using System;
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
        public Boolean UploadFile(string Path, FileStream File)
        {
            try
            {
                // Get the object used to communicate with the server.                
                Console.WriteLine(URL + ':' + Port + Path);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(URL + ':' + Port + Path);
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
                RemoveFile(Path);
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

        public void RemoveFile(string FileName)
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
