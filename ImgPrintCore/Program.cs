using System.Configuration;
using ImgPrint;
using ImgPrintCore;
using NDesk.Options;


bool show_help = false;

string dirPath = "";
string searchPatern = "*.png";
string printerName = "";
bool convertNeed = true;
string mediaType = "A4";
bool saveNeed = true;
bool deleteNeed = true;
int timerInterval = 1000;
int timerMaxTickCount = 0;
string printToPDFpath = "";
bool saveLog = true;
int dpi = 300;

string ShDirPath = "";
string ShSearchPatern = "*.xml";
string ShCommand = "";
string ShArgs = "";


fileSheduler fileSheduler = new fileSheduler();

ImgPrint.printEngine printEngine = new ImgPrint.printEngine();

try
{
    dirPath = ConfigurationManager.AppSettings["dirPath"];
    searchPatern = ConfigurationManager.AppSettings["searchPatern"];
    printerName = ConfigurationManager.AppSettings["printerName"];
    convertNeed = bool.Parse(ConfigurationManager.AppSettings["convertNeed"]);
    mediaType = ConfigurationManager.AppSettings["mediaType"];
    saveNeed = bool.Parse(ConfigurationManager.AppSettings["saveNeed"]);
    deleteNeed = bool.Parse(ConfigurationManager.AppSettings["deleteNeed"]);
    timerInterval = int.Parse(ConfigurationManager.AppSettings["timerInterval"]);
    timerMaxTickCount = int.Parse(ConfigurationManager.AppSettings["timerMaxTickCount"]);
    printToPDFpath = ConfigurationManager.AppSettings["printToPDFpath"];
    saveLog = bool.Parse(ConfigurationManager.AppSettings["saveLog"]);
    dpi = int.Parse(ConfigurationManager.AppSettings["dpi"]);

    ShDirPath = ConfigurationManager.AppSettings["ShDirPath"];
    ShSearchPatern = ConfigurationManager.AppSettings["ShSearchPatern"];
    ShCommand = ConfigurationManager.AppSettings["ShCommand"];
    ShArgs = ConfigurationManager.AppSettings["ShArgs"];

}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
}


if (args != null && args.Length > 0)
{
    setVarsFromOptions(args);
}


printEngine.dirPath = dirPath;
printEngine.searchPatern = searchPatern;
printEngine.printerName = printerName;
printEngine.mediaType = mediaType;
printEngine.convertNeed = convertNeed;
printEngine.saveNeed = saveNeed;
printEngine.deleteNeed = deleteNeed;
printEngine.timerInterval = timerInterval;
printEngine.timerMaxTickCount = timerMaxTickCount;
printEngine.printToPDFpath = printToPDFpath;
printEngine.saveLog = saveLog;
printEngine.dpi = dpi;

fileSheduler.command = ShCommand;
fileSheduler.args = ShArgs;
fileSheduler.dirPath = ShDirPath;
fileSheduler.searchPatern = ShSearchPatern;
fileSheduler.saveLog = saveLog;

fileSheduler.SetFileWatcher();

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
            {"c|convert=", "if need to convert images after printing",
              v => convertNeed = bool.Parse(v) },
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
            {"dpi=", "dpi ",
              v => dpi = int.Parse(v) },
            {"l|log=", "save log to file",
             v => saveLog = bool.Parse(v) },
              {"sd|sdir=", "directory for watching of executing files",
             v => ShDirPath = v },
            {"ss|ssearch=", "patern for searching of executing files",
              v => ShSearchPatern = v },
            {"sc|scom=", "shedule command",
              v => ShCommand = v },
            {"sa|sargs=", "shedule args",
              v => ShArgs = v },

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