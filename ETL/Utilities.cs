using System;
using System.Globalization;
using System.IO;
namespace ETL
{
    class Utilities
    {
        //Method to log any error or completed action
        public static void Logger(string message, string action = "")
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            if (action == "error")
            {
                // Write the string to a file named "ETLErrorLog.txt".
                using (StreamWriter outputFile = new StreamWriter(Utilities.BaseDirectory() + @"ETLErrorLog.txt", true))
                {                    
                    outputFile.WriteLine(System.DateTime.Now.ToString(culture) + ':' + message);
                }
            }
            else
            {
                // Write the string to a file named "ETLLog.txt".
                using (StreamWriter outputFile = new StreamWriter(Utilities.BaseDirectory() + @"ETLLog.txt", true))
                {
                    outputFile.WriteLine(System.DateTime.Now.ToString(culture) + ':' + message);
                }
            }
            Console.WriteLine(message);            
        }
        //Method to return Base directory of the execution file
        public static string BaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static bool HasValidArguments(string[] args, out string username, out string password, out string host, out int port, out bool hasCSVHeaders)
        {
            username = null;
            password = null;
            host = null;
            port = 21;
            hasCSVHeaders = false;
            try
            {
                foreach (string argument in args)
                {
                    string[] splitted = argument.Split('=');

                    if (splitted.Length == 2)
                    {
                        switch (splitted[0])
                        {
                            case "user":
                                username = splitted[1];
                                break;
                            case "password":
                                password = splitted[1];
                                break;
                            case "host":
                                host = splitted[1];
                                break;
                            case "port":
                                port = int.Parse(splitted[1]);
                                break;
                            case "includeheader":
                                hasCSVHeaders = bool.Parse(splitted[1]);
                                break;
                        };
                    }
                    else
                    {
                        Utilities.Logger("Error in parameters, please verify", "error");
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.Logger("Error with one or more parameters: \n" +
                                  "Number of parameters provided =" + args.Length +
                                  "\n Help: Port = (int number), includeheader = (true or false)",
                                  "error");
                foreach (string argument in args)
                {
                    Utilities.Logger(argument, "error");
                }
                throw ex;
            }
            return (username != null && password != null && host != null);
        }
    }
}
