using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ZipBackupApp
{

    public interface IFileZipper
    {
        public Response DailyFileZipper();
    }


    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    class FileModel
    {
        public string Name { get; set; }
        public string File { get; set; }
    }

    class PathModel
    {
        public string source { get; set; }
        public string destination { get; set; }
    }

    class ZipFileConfigModel
    {
        public PathModel path { get; set; }
        public string zipFileTmpFolderName { get; set; }
        public FileModel mondayBackup { get; set; }
        public FileModel tuesdayBackup { get; set; }
        public FileModel wednesdayBackup { get; set; }
        public FileModel thursdayBackup { get; set; }
        public FileModel fridayBackup { get; set; }
        public FileModel saturdayBackup { get; set; }
        public FileModel sundayBackup { get; set; }
    }

    public class FileZipper : IFileZipper
    {
        private readonly ZipFileConfigModel _zipFileConfig;
        private readonly string fileExt;

        public FileZipper()
        {
            _zipFileConfig = new ZipFileConfigModel()
            {
                path = new PathModel
                {
                    source = ConfigurationManager.AppSettings["SourceFolder"],
                    destination = ConfigurationManager.AppSettings["DestinationFolder"]
                },
                mondayBackup = new FileModel() { Name = "Monday", File = ConfigurationManager.AppSettings["MondayBackupFileName"] },
                tuesdayBackup = new FileModel() { Name = "Tuesday", File = ConfigurationManager.AppSettings["TuesdayBackupFileName"] },
                wednesdayBackup = new FileModel() { Name = "Wednesday", File = ConfigurationManager.AppSettings["WednesdayBackupFileName"] },
                thursdayBackup = new FileModel() { Name = "Thursday", File = ConfigurationManager.AppSettings["ThursdayBackupFileName"] },
                fridayBackup = new FileModel() { Name = "Thursday", File = ConfigurationManager.AppSettings["FridayBackupFileName"] },
                saturdayBackup = new FileModel() { Name = "Thursday", File = ConfigurationManager.AppSettings["SaturdayBackupFileName"] },
                sundayBackup = new FileModel() { Name = "Thursday", File = ConfigurationManager.AppSettings["SundayBackupFileName"] },
                
            };

            _zipFileConfig.zipFileTmpFolderName = Path.Combine(_zipFileConfig.path.destination, ConfigurationManager.AppSettings["ZipFileTmpFolderName"]);
            fileExt = ".zip";
        }

        public Response DailyFileZipper()
        {
            Response response = new Response() { Success = false };
            string sourceFile = GetSourceFullPath();

            if (!File.Exists(sourceFile))
            {
                response.Message = "Source file does not exist, operation aborted.................";
                WriteMessage(response.Message);
                return response;
            }

            WriteMessage("Source file confirmed....................");

            if (!Directory.Exists(_zipFileConfig.path.destination)) 
                    Directory.CreateDirectory(_zipFileConfig.path.destination);

            if (!SetUpTmpFolder())
            {
                response.Message = "Operation aborted.......................";
                WriteMessage(response.Message);
                return response;
            }

            var fileCopyFolder = CopyFileToTmp(sourceFile);

            if (string.IsNullOrEmpty(fileCopyFolder))
            {
                response.Message = "Operation aborted.......................";
                WriteMessage(response.Message);
                return response;
            }

            var zipFile = Path.Combine(_zipFileConfig.path.destination, Path.GetFileNameWithoutExtension(sourceFile) + fileExt);

            if (!CleanUpDestination(zipFile)) {
                response.Message = "Operation aborted, clean up not done.......................";
                WriteMessage(response.Message);
                return response;
            }

            var result = SaveFileToZip(fileCopyFolder, zipFile);

            if (result.Success)
            {
                //deete tmp 
                CleanUpTmpFolder();
            }

            response.Success = true;
            response.Message = "File successfully zipped......................";
            return response;

        }

        private bool SetUpTmpFolder()
        {

            if (Directory.Exists(_zipFileConfig.zipFileTmpFolderName))
            {
                WriteMessage(_zipFileConfig.zipFileTmpFolderName 
                    + " already exist, please configure a different tmp folder name in the App configuration file.");
                return false;
            }
            try
            {
                //create tmp
                Directory.CreateDirectory(_zipFileConfig.zipFileTmpFolderName);
            }
            catch (IOException io)
            {
                WriteMessage(io.Message);
                return false;
            }

            return true;
        }


        private bool CleanUpTmpFolder()
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(_zipFileConfig.zipFileTmpFolderName))
                {
                    foreach (var file in Directory.GetFiles(dir))
                    {
                        File.Delete(file);
                    }
                    Directory.Delete(dir);
                }

                Directory.Delete(_zipFileConfig.zipFileTmpFolderName);
            }
            catch (IOException io)
            {
                WriteMessage(io.Message);
                WriteMessage("Manually delete " + _zipFileConfig.zipFileTmpFolderName
                    + "to prevent subsequent operation failure.");
                return false;
            }

            return true;
        }

        private string CopyFileToTmp(string source)
        {
            var fileName = Path.GetFileNameWithoutExtension(source);
            var fileFolder = "";

            try
            {
                fileFolder = Path.Combine(_zipFileConfig.zipFileTmpFolderName, fileName);
                //if tmp folder exist delete and recreate 
                if (Directory.Exists(fileFolder)) Directory.Delete(fileFolder);

                Directory.CreateDirectory(fileFolder);

                WriteMessage("Copying file to " + fileFolder + ".");
                File.Copy(source, Path.Combine(fileFolder, fileName + Path.GetExtension(source)));
                WriteMessage("Copying completed.");

                return fileFolder;
            }
            catch (IOException io)
            {
                WriteMessage(io.Message);
                return null;
            }
        }

        private Response SaveFileToZip(string source, string destination)
        {
            Response response = new Response() { Success = false };

            try
            {
                WriteMessage("File compression in progress..................");
                ZipFile.CreateFromDirectory(source, destination);
                response.Message = "File compression completed..................";
                response.Success = true;
                WriteMessage(response.Message);
                return response;
            }
            catch (IOException io)
            {
                response.Message = io.Message;
                WriteMessage(response.Message);
                return response;
            }
        }

        private bool CleanUpDestination(string filePath)
        {
            if (!File.Exists(filePath)) return true;

            WriteMessage("Duplicate file exist in destination....................");
            var OverrideExistingFile = ConfigurationManager.AppSettings["OverrideExistingFile"].ToLower();

            if (OverrideExistingFile != "yes")
            {
                WriteMessage("OverrideExistiningFile is not set to yes.");
                WriteMessage("Operation Aborted.................");
                return false;
            }

            try
            {
                File.Delete(filePath);
                WriteMessage("Duplicate file deleted ....................");
                return true;
            }
            catch (IOException io)
            {
                WriteMessage(io.Message);
                return false;
            }
        }

        private string GetSourceFullPath()
        {
            var sourceFile = _zipFileConfig.path.source;
            DateTime today = DateTime.Today;

            if (today.DayOfWeek == DayOfWeek.Monday)
            {
                sourceFile = Path.Combine(sourceFile, _zipFileConfig.mondayBackup.File);
            }
            else if (today.DayOfWeek == DayOfWeek.Tuesday)
            {
                sourceFile = Path.Combine(sourceFile, _zipFileConfig.tuesdayBackup.File);
            }
            else if (today.DayOfWeek == DayOfWeek.Wednesday)
            {
                sourceFile = Path.Combine(sourceFile, _zipFileConfig.wednesdayBackup.File);
            }
            else if (today.DayOfWeek == DayOfWeek.Thursday)
            {
                sourceFile = Path.Combine(sourceFile, _zipFileConfig.thursdayBackup.File);
            }
            else if (today.DayOfWeek == DayOfWeek.Friday)
            {
                sourceFile = Path.Combine(sourceFile, _zipFileConfig.fridayBackup.File);
            }
            else if (today.DayOfWeek == DayOfWeek.Saturday)
            {
                sourceFile = Path.Combine(sourceFile, _zipFileConfig.saturdayBackup.File);
            }
            else if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                sourceFile = Path.Combine(sourceFile, _zipFileConfig.sundayBackup.File);
            }

            return sourceFile;
        }

        private void WriteMessage(string msg)
        {
            if (ConfigurationManager.AppSettings["WriteToLog"].ToLower() == "no")
            {
                WriteMessageToConsole(msg);
            }
            else if (ConfigurationManager.AppSettings["WriteToLog"].ToLower() == "yes")
            {
                WriteMessageToLog(msg);
            }
            return;
        }

        private void WriteMessageToConsole(string msg)
        {
            Console.WriteLine("");
            Console.WriteLine(msg);
            Console.WriteLine("");
        }

        private void WriteMessageToLog(string msg)
        {
            ILogger logger = new Logger();
            logger.Information(msg);
            return;
        }

    }
 
}
