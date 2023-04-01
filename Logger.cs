using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZipBackupApp
{

    public interface ILogger
    {
        public void Information(string msg);
    }

    class Logger : ILogger
    {
        private readonly string _loggerRoot;

        public Logger()
        {
            _loggerRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            //string path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            //_loggerRoot = Path.Combine(path, "Logs");

        }

        public void Information(string msg)
        {
            CreatLogFileIfNotExists();

            var file = GetCurrentFile();

            string imsg = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + ": " + msg;

            if (File.Exists(file))
            {
                FileStream filestream = new FileStream(@file, FileMode.Open, FileAccess.ReadWrite);
                filestream.Seek(0, SeekOrigin.End);

                StreamWriter streamWriter = new StreamWriter(filestream);
                streamWriter.WriteLine(imsg);

                streamWriter.Close();
                filestream.Close();
            }
            
            return;
        }

        private void CreatLogFileIfNotExists()
        {
            var file = GetCurrentFile();
            if (File.Exists(file)) return;

            if (!Directory.Exists(_loggerRoot)) Directory.CreateDirectory(_loggerRoot);

            using (FileStream fileStream = File.Create(file)) return;
        }

        private string GetCurrentFile()
        {
            return Path.Combine(_loggerRoot, "Log_" + DateTime.Now.ToString("dd_MM_yyyy")+".txt");
        }
    }
}
