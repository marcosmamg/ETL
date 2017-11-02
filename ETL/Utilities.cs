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
        public static void LogError(string strError)
        {
            // Write the string to a file named "ETLLog.txt".
            using (StreamWriter outputFile = new StreamWriter(Utilities.BaseDirectory() + @"ETLLog.txt", true))
            {
                outputFile.WriteLine(System.DateTime.Now.ToShortDateString() + ':' + strError);
            }
        }
        public static string BaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
