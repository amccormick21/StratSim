using System;
using System.Collections.Generic;
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
            return (string)Properties.Settings.Default["StratSimConnectionString"];
        }
    }
}
