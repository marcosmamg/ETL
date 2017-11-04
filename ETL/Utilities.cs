using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETL
{
    class Utilities
    {
        public static void Log(string strError, string action = "")
        {
            if (action == "error")
            {
                // Write the string to a file named "ETLErrorLog.txt".
                using (StreamWriter outputFile = new StreamWriter(Utilities.BaseDirectory() + @"ETLErrorLog.txt", true))
                {
                    outputFile.WriteLine(System.DateTime.Now.ToString() + ':' + strError);
                }
            }
            else
            {
                // Write the string to a file named "ETLLog.txt".
                using (StreamWriter outputFile = new StreamWriter(Utilities.BaseDirectory() + @"ETLLog.txt", true))
                {
                    outputFile.WriteLine(System.DateTime.Now.ToString() + ':' + strError);
                }
            }
            
        }
        public static string BaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
