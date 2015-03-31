using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSources
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Do Nothing
        }

        public static string GetConnectionString()
        {
            var path = Environment.CurrentDirectory;
            //Go to the third level above the current directory
            for (int i = 0; i < 3; i++)
            {
                path = Path.GetDirectoryName(path);
            }
            path = Path.Combine(path, "Data");
            var connectionString = (string)Properties.Settings.Default["StratSimConnectionString"];
            connectionString = connectionString.Replace("|DataDirectory|", path);
            return connectionString;
        }
    }
}
