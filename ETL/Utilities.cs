using System;
using System.Globalization;
using System.IO;
namespace ETL
{
    static class Utilities 
    {   
        //Method to log any error or completed action
        public static void Logger(string message, string action = "")
        {
            try
            {
                CultureInfo culture = CultureInfo.CurrentCulture;
                string folder = Utilities.BaseDirectory() + "Logs/";

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                if (action == "error")
                {
                    using (StreamWriter outputFile = new StreamWriter(folder + @"ETLErrorLog" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true))
                    {
                        outputFile.WriteLine(System.DateTime.Now.ToString(culture) + ':' + message);
                    }
                }
                else
                {
                    using (StreamWriter outputFile = new StreamWriter(folder + @"ETLLog" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true))
                    {
                        outputFile.WriteLine(System.DateTime.Now.ToString(culture) + ':' + message);
                    }
                }

                Console.WriteLine(message);
            }
            catch{}            
        }     

        //Method to return Base directory of the execution file
        public static string BaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }       
    }
}
