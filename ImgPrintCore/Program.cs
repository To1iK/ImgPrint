using System.Configuration;
using NDesk.Options;


bool show_help = false;

string dirPath;
string searchPatern;
string printerName;
string mediaType;
bool saveNeed;
bool deleteNeed;
int timerInterval;
int timerMaxTickCount;
string printToPDFpath;
bool saveLog;

dirPath = ConfigurationManager.AppSettings["dirPath"];
searchPatern = ConfigurationManager.AppSettings["searchPatern"];
printerName = ConfigurationManager.AppSettings["printerName"];
mediaType = ConfigurationManager.AppSettings["mediaType"];
saveNeed = bool.Parse(ConfigurationManager.AppSettings["saveNeed"]);
deleteNeed = bool.Parse(ConfigurationManager.AppSettings["deleteNeed"]);
timerInterval = int.Parse(ConfigurationManager.AppSettings["timerInterval"]);
timerMaxTickCount = int.Parse(ConfigurationManager.AppSettings["timerMaxTickCount"]);
printToPDFpath = ConfigurationManager.AppSettings["printToPDFpath"];
saveLog = bool.Parse(ConfigurationManager.AppSettings["saveLog"]);

if (args != null && args.Length > 0)
{
    setVarsFromOptions(args);
}


ImgPrint.printEngine printEngine = new ImgPrint.printEngine();

printEngine.dirPath = dirPath;
printEngine.searchPatern = searchPatern;
printEngine.printerName = printerName;
printEngine.mediaType = mediaType;
printEngine.saveNeed = saveNeed;
printEngine.deleteNeed = deleteNeed;
printEngine.timerInterval = timerInterval;
printEngine.timerMaxTickCount = timerMaxTickCount;
printEngine.printToPDFpath = printToPDFpath;
printEngine.saveLog = saveLog;

printEngine.Printing();

Console.ReadLine();


 void setVarsFromOptions(string[] args)
{

    var optionSet = new OptionSet() {

            {"h|help",  "help",
              v => show_help = v != null },

            {"d|dir=", "directory with images for printing",
              v => dirPath = v },
            {"s|search=", "patern for searching of files in dir",
              v => searchPatern = v },
            {"p|printer=", "name of printer with windows driver",
              v => printerName = v },
            {"m|mtype=", "name of mediatype for printing",
              v => mediaType = v },
            {"s|save=", "if need to save converted images after printing",
              v => saveNeed = bool.Parse(v) },
            {"dt|delete=", "if need to delete images from working dir",
              v => deleteNeed = bool.Parse(v) },
            {"i|interval=", "timer interval for checkinf if new images was added",
              v => timerInterval = int.Parse(v) },
            {"t|tiks=", "how mach ticks of timer programs shold check new images",
              v => timerMaxTickCount = int.Parse(v) },
            {"pdf=", "path to dir for saving docoment if pdf(print to file) printer is using",
              v => printToPDFpath = v },
            {"l|log=", "save log to file",
              v => saveLog = bool.Parse(v) },
            };

    List<string> extra;
    try
    {
        extra = optionSet.Parse(args);
        Console.WriteLine(extra.Count);
    }
    catch (OptionException e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine("Please use `--help' to get options info ");
        return;
    }

    if (show_help)
    {
        ShowHelp(optionSet);
        return;
    }
}


void ShowHelp(OptionSet option)
{
    Console.WriteLine("ImgPrint");
    Console.WriteLine("Options:");
    option.WriteOptionDescriptions(Console.Out);
}