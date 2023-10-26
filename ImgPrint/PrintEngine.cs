using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace ImgPrint
{
    public class PrintingExample
    {
       private StreamReader streamToPrint;
       static string filePath;
       public string dirPath= "";
       public string searchPatern = "*.png";
       public string printerName = "";
       public bool saveNeed = false;
       public bool deleteNeed = false;
       public int dpi = 300;
       public string mediaType = "A4";
       public bool convertNeed = true;

       public int timerInterval = 2000;
       public int timerMaxTickCount = 7;

       public string printToPDFpath = "";

        Dictionary<string, printedFile> printFiles;

        private static System.Timers.Timer aTimer;
        int t = 0;
      

        public void Printing()
        {
               
            printFiles = new Dictionary<string,printedFile> ();

            if (!Directory.Exists(dirPath))
            { 
                log($"No such directory - {dirPath}");
                Environment.Exit(0);
            }

            foreach (var f in Directory.GetFiles(dirPath, searchPatern))
            {
                string fn = Path.GetFileName(f);
                if (!printFiles.ContainsKey(fn))                 
                        printFiles.Add(fn, new printedFile(f));                             
            }
                       
            if (timerInterval > 0)
            {
                
                SetFileWatcher();
                t = 0;
                aTimer = new System.Timers.Timer(timerInterval);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
            else
            {
                printImgList(printFiles.Values.Where(x=>x.status== printFileStatus.added).ToArray());
            }            
             
        }

        public void printArrayOfFiles(string files, char delimeter=';')
        {
            string[] sf = files.Split(delimeter);
            printImgList(printFiles.Values.Where(x => x.status == printFileStatus.added).ToArray());
            for (var n = 0; n < sf.Length; n++)
            {                
                string fn = Path.GetFileName(sf[n]);
                if (!printFiles.ContainsKey(fn))
                    printFiles.Add(fn, new printedFile(sf[n]));
            }

            printImgList(printFiles.Values.Where(x => x.status == printFileStatus.added).ToArray());

        }

        public void printArrayOfFilesInDir(string dir, string files, char delimeter = ';')
        {
            
            string[] sf = files.Split(delimeter);

            printFiles = new Dictionary<string, printedFile>();

          for (var n=0;n<sf.Length;n++)
            {
                sf[n] = dir + "\\" + sf[n];
                string fn = Path.GetFileName(sf[n]);
                if (!printFiles.ContainsKey(fn))
                    printFiles.Add(fn, new printedFile(sf[n]));
            }
            printImgList(printFiles.Values.Where(x => x.status == printFileStatus.added).ToArray());
        }

        public void printImgList(printedFile[] files)
        {

            if (files == null || files.Length == 0)
            {
               // log("no files for printing");
                return;
            }

          foreach (var f in files)
            {
                f.status = printFileStatus.in_queue;
            }

         int n = 0;

            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printerName;
            
            PaperSize pkCustomSize = pd.PrinterSettings
                .PaperSizes.Cast<PaperSize>()
                .Where(x => x.PaperName == mediaType)
                .FirstOrDefault();

            if (pkCustomSize == null)
            {
                log($"No such media type for windows printer - ${mediaType}");
            }

            else
            {
                pd.DefaultPageSettings.PaperSize = pkCustomSize;
            }
                      

            if (printToPDFpath !=null && printToPDFpath != "")
            {
            pd.PrinterSettings.PrintToFile = true;
            pd.PrinterSettings.PrintFileName = $"{printToPDFpath}\\{DateTime.Now.Month}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.pdf";
            }

            if (convertNeed && Directory.Exists($"{dirPath}\\converted") == false)
            {
                try
                {
                    Directory.CreateDirectory($"{dirPath}\\converted");
                }
                catch (Exception ex)
                {
                    log(ex.Message);
                }
            }

            pd.PrintPage += (sender, args) =>
            {

                printedFile pf = files[n];
                try
                {

                    Bitmap bmpFromFile = new Bitmap(pf.filePath);

                    if (convertNeed)
                    {
                        Bitmap bmp1b = bmpFromFile.Clone(new Rectangle(0, 0, bmpFromFile.Width, bmpFromFile.Height),
                        PixelFormat.Format1bppIndexed);

                    bmp1b.SetResolution(dpi, dpi);

                    if (saveNeed)
                        bmp1b.Save($"{dirPath}\\converted\\{Path.GetFileNameWithoutExtension(pf.filePath)}.bmp", ImageFormat.Bmp);

                    args.Graphics.DrawImage(bmp1b, new Point(0, 0));

                    bmp1b.Dispose();
                    }
                    else
                    {
                        args.Graphics.DrawImage(bmpFromFile, new Point(0, 0));
                    }

                    bmpFromFile.Dispose();


                    if (n >= files.Length - 1)
                    args.HasMorePages = false;
                else
                    args.HasMorePages = true;

                pf.status = printFileStatus.printed;

                }
                catch(Exception ex)
                {
                    log(ex.Message);
                    pf.status = printFileStatus.hasErrors;
                }
                finally
                {
                n++;
                }
               
                
            };

            log($"Start printing");
            
            pd.Print();
            log($"End printing");
            pd.Dispose();
           
                if (deleteNeed)
                {
                    foreach(var f in files)
                     {
                    //log(f.status.ToString());
                    System.IO.File.Delete(f.filePath);
                    printFiles.Remove(Path.GetFileName(f.filePath));                          

                     }
            }
                     

        }

        private void SetFileWatcher()
        {
            var watcher = new FileSystemWatcher(dirPath);
            watcher.NotifyFilter = NotifyFilters.Attributes
                          | NotifyFilters.Size
                           ;

            watcher.Changed += OnChanged;

            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
            log("FileWather started");
        }

        void log(string msg)
        {
            Console.WriteLine($"{DateTime.Now} - {msg}");
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");

            string fn = Path.GetFileName(e.FullPath);
            if (!printFiles.ContainsKey(fn))
                printFiles.Add(fn, new printedFile(e.FullPath));
        } 

       

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            t++;
            Console.WriteLine("Tick #{0}",t);
           
            if (timerMaxTickCount>0 && t >= timerMaxTickCount)
            {
                aTimer.Stop();
                Environment.Exit(0);
            }
            
            Thread th = new Thread(new ParameterizedThreadStart(printAsync));
           
            th.Start(printFiles);
                       
        }

        void printAsync(object list)
        {
            printImgList(((Dictionary<string, printedFile>)list)
                .Values
                .Where(x => x.status == printFileStatus.added)
                .ToArray());
        }

        void convertAsync(object list)
        {
            if(Directory.Exists($"{dirPath}\\converted") == false)
            {
                try
                {
 Directory.CreateDirectory($"{dirPath}\\converted");
                }
               catch(Exception ex)
                {
                    log(ex.Message);
                }
            }

          foreach (var f in ((Dictionary<string, printedFile>)list)
                            .Values
                            .Where(x => x.status == printFileStatus.added)
                            .ToArray())
            {

                Bitmap bmpFromFile = new Bitmap(f.filePath);
                              
                    f.bmp1b = bmpFromFile.Clone(new Rectangle(0, 0, bmpFromFile.Width, bmpFromFile.Height),
                    PixelFormat.Format1bppIndexed);

                    f.bmp1b.SetResolution(dpi, dpi);

                    f.status = printFileStatus.converted;

                    if (saveNeed)
                        f.bmp1b.Save($"{dirPath}\\converted\\{Path.GetFileNameWithoutExtension(f.filePath)}.bmp", ImageFormat.Bmp);
           }
        }
    }


   public class printedFile:IDisposable
    {
        public string filePath;
        public printFileStatus status;
        public Bitmap bmp1b;

       public printedFile(string filePath, printFileStatus status)
        {
            this.filePath = filePath;
            this.status = status;
        }
        public printedFile(string filePath)
        {
            this.filePath = filePath;
            this.status = printFileStatus.added;
        }

        public void Dispose()
        {
            bmp1b = null;
            bmp1b.Dispose();
        }
    }

   public enum printFileStatus
    {
        hasErrors,
        added,
        in_queue,
        converted,
        printed,
        removed
    }
}
