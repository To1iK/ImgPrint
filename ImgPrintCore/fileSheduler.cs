using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgPrintCore
{
    internal class fileSheduler
    {
      public string dirPath = "";
      public  string searchPatern = ".xml";
      public  bool saveLog = true;
      public  string command = "";
      public string args = "";

        FileSystemWatcher watcher = new FileSystemWatcher();
        public void SetFileWatcher()
        {
            //  var watcher = new FileSystemWatcher(dirPath, searchPatern);
            watcher.NotifyFilter = NotifyFilters.Attributes
                          | NotifyFilters.Size
                           ;
            watcher.Path = dirPath;
            watcher.Filter = searchPatern;

            watcher.Changed += OnChanged;

            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;

            log($"fileSheduler started - {watcher.Path}");
        }

        void log(string msg)
        {

            if (saveLog == true)
            {
                System.IO.File.AppendAllText(Environment.CurrentDirectory + "/log.txt", $"{DateTime.Now} - {msg}\r\n");
            }
            else
            {
                Console.WriteLine($"{DateTime.Now} - {msg}");
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
#if DEBUG
            Console.WriteLine(e.Name);
#endif
            if (e.ChangeType == WatcherChangeTypes.Changed
                && File.Exists(e.FullPath) == true
                && IsFileLocked(e.FullPath)==false 
                )
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo(command, args);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                using (Process process = new Process())
                {
                    process.StartInfo = procStartInfo;
                    process.Start();

                    process.WaitForExit();

                    string result = process.StandardOutput.ReadToEnd();
                    log(result);

                }

                File.Delete(e.FullPath);
            }
            //Console.WriteLine($"Changed: {e.FullPath}");
           // "'C:\Program Files (x86)\Marel\Innova\bin\procop.exe' -o exuo -f E:\scanner_ftp\test\data.xml"
         

           
        }

        private bool IsFileLocked(string f)
        {
            FileInfo file = new FileInfo(f);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {              
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
         
            return false;
        }
    }
}
