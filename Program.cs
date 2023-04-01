using System;
using System.Configuration;

namespace ZipBackupApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger = new Logger();

            string msg = "Program Initialized.............................";

            if (ConfigurationManager.AppSettings["WriteToLog"].ToLower() == "yes") logger.Information(msg);
            else Console.WriteLine(msg);

            IFileZipper zipFile = new FileZipper();
            var r= zipFile.DailyFileZipper();

            msg = "Program out.....................................";

            if (ConfigurationManager.AppSettings["WriteToLog"].ToLower() == "yes") logger.Information(msg);
            else Console.WriteLine(msg);
            //var wait = Console.ReadKey();
        }
    }
}
