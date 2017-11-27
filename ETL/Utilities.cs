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
                using (StreamWriter outputFile = new StreamWriter(Utilities.BaseDirectory() + "Logs/" + @"ETLErrorLog" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true))
                {                    
                    outputFile.WriteLine(System.DateTime.Now.ToString(culture) + ':' + message);
                }
            }
            else
            {
                // Write the string to a file named "ETLLog.txt".
                using (StreamWriter outputFile = new StreamWriter(Utilities.BaseDirectory() + "Logs/" + @"ETLLog" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true))
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
    }
}
