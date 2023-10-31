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
    public class printEngine
    {
       
       //static string filePath;
       public string dirPath = "";
       public string searchPatern = "";
       public string printerName;
       public bool saveNeed;
       public bool deleteNeed;
       public int dpi;
       public string mediaType;
       public bool convertNeed;
       public bool isConvertAsync;

       public int timerInterval;
       public int timerMaxTickCount;

       public string printToPDFpath ;
       public bool saveLog ;

        Dictionary<string, printFile> printFiles;

        private static System.Timers.Timer aTimer;
        int t = 0;
        private bool removeXML;

        PrinterSettings ps;
        private IEnumerable<PaperSize> paperSizes;

        public void Printing()
        {
               
            printFiles = new Dictionary<string,printFile> ();
            ps = new PrinterSettings();
            ps.PrinterName = printerName;
            if (paperSizes == null)
            {
                paperSizes = ps
                .PaperSizes
                .Cast<PaperSize>();
            }

            if (printToPDFpath != null && printToPDFpath != "")
            {
                ps.PrintToFile = true;
                ps.PrintFileName = $"{printToPDFpath}/{DateTime.Now.Month}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.pdf";
            }

            if (!Directory.Exists(dirPath))
            { 
                log($"No such directory - {dirPath}");
                Environment.Exit(0);
            }

            foreach (var f in Directory.GetFiles(dirPath, searchPatern))
            {
                string fn = Path.GetFileName(f);
                if (!printFiles.ContainsKey(fn))                 
                        printFiles.Add(fn, new printFile(f));               
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
                    printFiles.Add(fn, new printFile(sf[n]));
            }

            printImgList(printFiles.Values.Where(x => x.status == printFileStatus.added).ToArray());

        }

        public void printArrayOfFilesInDir(string dir, string files, char delimeter = ';')
        {
            
            string[] sf = files.Split(delimeter);

            printFiles = new Dictionary<string, printFile>();

          for (var n=0;n<sf.Length;n++)
            {
                sf[n] = dir + "/" + sf[n];
                string fn = Path.GetFileName(sf[n]);
                if (!printFiles.ContainsKey(fn))
                    printFiles.Add(fn, new printFile(sf[n]));
            }
            printImgList(printFiles.Values.Where(x => x.status == printFileStatus.added).ToArray());
        }

        public void printImgList(printFile[] files)
        {
            Console.WriteLine($"step 1 - {DateTime.Now.ToString("mm.ss.fff")}");
            if (files == null || files.Length == 0)
            {
               // log("no files for printing");
                return;
            }

            foreach (var f in files)
            {
                f.status = printFileStatus.in_queue;
            }
         
            if (isConvertAsync)
            {
                Thread t = new Thread(new ThreadStart(convertAsync));
                t.Start();
            }

         int n = 0;
  
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings = ps;

           PaperSize pkCustomSize = paperSizes
                .Where(x => x.PaperName == mediaType)
                .FirstOrDefault();

            if (pkCustomSize == null)
            {
                log($"No such media type for windows printer - ${mediaType}");
                Bitmap bm = new Bitmap(files[0].filePath);
                pkCustomSize = new PaperSize("custom"
                                             ,(int)Math.Ceiling((double)(bm.Width * 100) / dpi)
                                             ,(int)Math.Ceiling((double)(bm.Height * 100) / dpi));

                bm.Dispose();
                bm = null;

            }
                         
            pd.DefaultPageSettings.PaperSize = pkCustomSize;
           

            if (convertNeed && Directory.Exists($"{dirPath}/converted") == false)
            {
                try
                {
                    Directory.CreateDirectory($"{dirPath}/converted");
                }
                catch (Exception ex)
                {
                    log(ex.Message + " - " + ex.StackTrace);
                }
            }
                        
            pd.PrintPage += (sender, args) =>
            {

                printFile pf = files[n];
                try
                {
                    if (convertNeed)
                    {
                        if (isConvertAsync && pf.bmp1b!=null)
                        {                       
                            args.Graphics.DrawImage(pf.bmp1b, new Point(0, 0));
                            pf.bmp1b.Dispose();
                            pf.bmp1b = null;
                        }
                        else
                        {

                        Bitmap bmpFromFile = new Bitmap(pf.filePath);
                        Bitmap bmp1b = bmpFromFile.Clone(new Rectangle(0, 0, bmpFromFile.Width, bmpFromFile.Height),
                        PixelFormat.Format1bppIndexed);

                        bmp1b.SetResolution(dpi, dpi);

                        if (saveNeed)
                        bmp1b.Save($"{dirPath}/converted/{Path.GetFileNameWithoutExtension(pf.filePath)}.bmp", ImageFormat.Bmp);

                        args.Graphics.DrawImage(bmp1b, new Point(0, 0));

                        bmp1b.Dispose();
                        bmpFromFile.Dispose();
                        }
                       
                    }
                    else
                    {
                        Bitmap bmpFromFile = new Bitmap(pf.filePath);
                        args.Graphics.DrawImage(bmpFromFile, new Point(0, 0));
                        bmpFromFile.Dispose();
                    }

                   


               if (n >= files.Length - 1)
                    args.HasMorePages = false;
                else
                    args.HasMorePages = true;

                pf.status = printFileStatus.printed;

                }
                catch (Exception ex)
                {
                    log($"printing - {ex.Message} - {ex.StackTrace}");
                    pf.status = printFileStatus.hasErrors;
                }

                n++;       
                
            };

            log($"Start printing of {files.Length} page(s)");
            
            pd.Print();
            log($"End printing of {files.Length} page(s)");

            pd.Dispose();

            if (deleteNeed)
            {
                foreach (var f in files)
                {
                    //log($"{f.filePath.ToString()} - {f.status}");
                    System.IO.File.Delete(f.filePath);
                    printFiles.Remove(Path.GetFileName(f.filePath));
                    string xml = $"{dirPath}/{Path.GetFileNameWithoutExtension(f.filePath)}.xml";
                    if (System.IO.File.Exists(xml))
                    {
                        System.IO.File.Delete(xml);
                    }
                }
            }


        }

        FileSystemWatcher watcher = new FileSystemWatcher();
        private void SetFileWatcher()
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
            
            log($"FileWather started - {watcher.Path}");
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
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            //Console.WriteLine($"Changed: {e.FullPath}");

            string fn = Path.GetFileName(e.FullPath);
            if (!printFiles.ContainsKey(fn))
                printFiles.Add(fn, new printFile(e.FullPath));
        } 

       

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            t++;
#if DEBUG
            Console.WriteLine("Tick #{0}",t);
#endif          
            if (timerMaxTickCount>0 && t >= timerMaxTickCount)
            {
                aTimer.Stop();
                Environment.Exit(0);
            }
            
            Thread th = new Thread(new ParameterizedThreadStart(printAsync));
           
            th.Start(printFiles);

            //System.IO.File.AppendAllText(Environment.CurrentDirectory + "/log.txt", sb.ToString());

            //sb.Clear();

        }

        void printAsync(object list)
        {
            printImgList(((Dictionary<string, printFile>)list)
                .Values
                .Where(x => x.status == printFileStatus.added)
                .ToArray());
        }

        void convertAsync()
        {
            if(Directory.Exists($"{dirPath}/converted") == false)
            {
                try
                {
                Directory.CreateDirectory($"{dirPath}/converted");
                }
               catch(Exception ex)
                {
                    log($"convertAsync - {ex.Message}");
                }
            }

          foreach (var f in printFiles
                            .Values
                            .Where(x => x.status == printFileStatus.added)
                            .ToArray())
            {

                    Bitmap bmpFromFile = new Bitmap(f.filePath);
                              
                    f.bmp1b = bmpFromFile.Clone(new Rectangle(0, 0, bmpFromFile.Width, bmpFromFile.Height),
                    PixelFormat.Format1bppIndexed);

                    f.bmp1b.SetResolution(dpi, dpi);

                    //f.status = printFileStatus.converted;
               
                    if (saveNeed)
                        f.bmp1b.Save($"{dirPath}/converted/{Path.GetFileNameWithoutExtension(f.filePath)}.bmp", ImageFormat.Bmp);
           }
        }
    }


  

 
}
