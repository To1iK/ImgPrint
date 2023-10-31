using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Compatibility.VB6;

namespace ImgPrint
{
    //
    // Summary:
    //     Defines a reusable object that sends output to a printer, when printing from
    //     a Windows Forms application.
    [ToolboxItemFilter("System.Drawing.Printing")]
    [DefaultProperty("DocumentName")]
    [SRDescription("PrintDocumentDesc")]
    [DefaultEvent("PrintPage")]
    public class PrintDocument : Component
    {
        private string documentName = "document";

        private PrintEventHandler beginPrintHandler;

        private PrintEventHandler endPrintHandler;

        private PrintPageEventHandler printPageHandler;

        private QueryPageSettingsEventHandler queryHandler;

        private PrinterSettings printerSettings = new PrinterSettings();

        private PageSettings defaultPageSettings;

        private PrintController printController;

        private bool originAtMargins;

        private bool userSetPageSettings;

        //
        // Summary:
        //     Gets or sets page settings that are used as defaults for all pages to be printed.
        //
        // Returns:
        //     A System.Drawing.Printing.PageSettings that specifies the default page settings
        //     for the document.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SRDescription("PDOCdocumentPageSettingsDescr")]
        public PageSettings DefaultPageSettings
        {
            get
            {
                return defaultPageSettings;
            }
            set
            {
                if (value == null)
                {
                    value = new PageSettings();
                }

                defaultPageSettings = value;
                userSetPageSettings = true;
            }
        }

        //
        // Summary:
        //     Gets or sets the document name to display (for example, in a print status dialog
        //     box or printer queue) while printing the document.
        //
        // Returns:
        //     The document name to display while printing the document. The default is "document".
        [DefaultValue("document")]
        [SRDescription("PDOCdocumentNameDescr")]
        public string DocumentName
        {
            get
            {
                return documentName;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }

                documentName = value;
            }
        }

        //
        // Summary:
        //     Gets or sets a value indicating whether the position of a graphics object associated
        //     with a page is located just inside the user-specified margins or at the top-left
        //     corner of the printable area of the page.
        //
        // Returns:
        //     true if the graphics origin starts at the page margins; false if the graphics
        //     origin is at the top-left corner of the printable page. The default is false.
        [DefaultValue(false)]
        [SRDescription("PDOCoriginAtMarginsDescr")]
        public bool OriginAtMargins
        {
            get
            {
                return originAtMargins;
            }
            set
            {
                originAtMargins = value;
            }
        }

        //
        // Summary:
        //     Gets or sets the print controller that guides the printing process.
        //
        // Returns:
        //     The System.Drawing.Printing.PrintController that guides the printing process.
        //     The default is a new instance of the System.Windows.Forms.PrintControllerWithStatusDialog
        //     class.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SRDescription("PDOCprintControllerDescr")]
        public PrintController PrintController
        {
            get
            {
                IntSecurity.SafePrinting.Demand();
                if (printController == null)
                {
                    printController = new StandardPrintController();
                    new ReflectionPermission(PermissionState.Unrestricted).Assert();
                    try
                    {
                        Type type = Type.GetType("System.Windows.Forms.PrintControllerWithStatusDialog, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                        printController = (PrintController)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new object[1] { printController }, null);
                    }
                    catch (TypeLoadException)
                    {
                    }
                    catch (TargetInvocationException)
                    {
                    }
                    catch (MissingMethodException)
                    {
                    }
                    catch (MethodAccessException)
                    {
                    }
                    catch (MemberAccessException)
                    {
                    }
                    catch (FileNotFoundException)
                    {
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }

                return printController;
            }
            set
            {
                IntSecurity.SafePrinting.Demand();
                printController = value;
            }
        }

        //
        // Summary:
        //     Gets or sets the printer that prints the document.
        //
        // Returns:
        //     A System.Drawing.Printing.PrinterSettings that specifies where and how the document
        //     is printed. The default is a System.Drawing.Printing.PrinterSettings with its
        //     properties set to their default values.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SRDescription("PDOCprinterSettingsDescr")]
        public PrinterSettings PrinterSettings
        {
            get
            {
                return printerSettings;
            }
            set
            {
                if (value == null)
                {
                    value = new PrinterSettings();
                }

                printerSettings = value;
                if (!userSetPageSettings)
                {
                    defaultPageSettings = printerSettings.DefaultPageSettings;
                }
            }
        }

        //
        // Summary:
        //     Occurs when the System.Drawing.Printing.PrintDocument.Print method is called
        //     and before the first page of the document prints.
        [SRDescription("PDOCbeginPrintDescr")]
        public event PrintEventHandler BeginPrint
        {
            add
            {
                beginPrintHandler = (PrintEventHandler)Delegate.Combine(beginPrintHandler, value);
            }
            remove
            {
                beginPrintHandler = (PrintEventHandler)Delegate.Remove(beginPrintHandler, value);
            }
        }

        //
        // Summary:
        //     Occurs when the last page of the document has printed.
        [SRDescription("PDOCendPrintDescr")]
        public event PrintEventHandler EndPrint
        {
            add
            {
                endPrintHandler = (PrintEventHandler)Delegate.Combine(endPrintHandler, value);
            }
            remove
            {
                endPrintHandler = (PrintEventHandler)Delegate.Remove(endPrintHandler, value);
            }
        }

        //
        // Summary:
        //     Occurs when the output to print for the current page is needed.
        [SRDescription("PDOCprintPageDescr")]
        public event PrintPageEventHandler PrintPage
        {
            add
            {
                printPageHandler = (PrintPageEventHandler)Delegate.Combine(printPageHandler, value);
            }
            remove
            {
                printPageHandler = (PrintPageEventHandler)Delegate.Remove(printPageHandler, value);
            }
        }

        //
        // Summary:
        //     Occurs immediately before each System.Drawing.Printing.PrintDocument.PrintPage
        //     event.
        [SRDescription("PDOCqueryPageSettingsDescr")]
        public event QueryPageSettingsEventHandler QueryPageSettings
        {
            add
            {
                queryHandler = (QueryPageSettingsEventHandler)Delegate.Combine(queryHandler, value);
            }
            remove
            {
                queryHandler = (QueryPageSettingsEventHandler)Delegate.Remove(queryHandler, value);
            }
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Drawing.Printing.PrintDocument class.
        public PrintDocument()
        {
            defaultPageSettings = new PageSettings(printerSettings);
        }

        internal void _OnBeginPrint(PrintEventArgs e)
        {
            OnBeginPrint(e);
        }

        //
        // Summary:
        //     Raises the System.Drawing.Printing.PrintDocument.BeginPrint event. It is called
        //     after the System.Drawing.Printing.PrintDocument.Print method is called and before
        //     the first page of the document prints.
        //
        // Parameters:
        //   e:
        //     A System.Drawing.Printing.PrintEventArgs that contains the event data.
        protected virtual void OnBeginPrint(PrintEventArgs e)
        {
            if (beginPrintHandler != null)
            {
                beginPrintHandler(this, e);
            }
        }

        internal void _OnEndPrint(PrintEventArgs e)
        {
            OnEndPrint(e);
        }

        //
        // Summary:
        //     Raises the System.Drawing.Printing.PrintDocument.EndPrint event. It is called
        //     when the last page of the document has printed.
        //
        // Parameters:
        //   e:
        //     A System.Drawing.Printing.PrintEventArgs that contains the event data.
        protected virtual void OnEndPrint(PrintEventArgs e)
        {
            if (endPrintHandler != null)
            {
                endPrintHandler(this, e);
            }
        }

        internal void _OnPrintPage(PrintPageEventArgs e)
        {
            OnPrintPage(e);
        }

        //
        // Summary:
        //     Raises the System.Drawing.Printing.PrintDocument.PrintPage event. It is called
        //     before a page prints.
        //
        // Parameters:
        //   e:
        //     A System.Drawing.Printing.PrintPageEventArgs that contains the event data.
        protected virtual void OnPrintPage(PrintPageEventArgs e)
        {
            if (printPageHandler != null)
            {
                printPageHandler(this, e);
            }
        }

        internal void _OnQueryPageSettings(QueryPageSettingsEventArgs e)
        {
            OnQueryPageSettings(e);
        }

        //
        // Summary:
        //     Raises the System.Drawing.Printing.PrintDocument.QueryPageSettings event. It
        //     is called immediately before each System.Drawing.Printing.PrintDocument.PrintPage
        //     event.
        //
        // Parameters:
        //   e:
        //     A System.Drawing.Printing.QueryPageSettingsEventArgs that contains the event
        //     data.
        protected virtual void OnQueryPageSettings(QueryPageSettingsEventArgs e)
        {
            if (queryHandler != null)
            {
                queryHandler(this, e);
            }
        }

        //
        // Summary:
        //     Starts the document's printing process.
        //
        // Exceptions:
        //   T:System.Drawing.Printing.InvalidPrinterException:
        //     The printer named in the System.Drawing.Printing.PrinterSettings.PrinterName
        //     property does not exist.
        public void Print()
        {
            if (!PrinterSettings.IsDefaultPrinter && !PrinterSettings.PrintDialogDisplayed)
            {
                IntSecurity.AllPrinting.Demand();
            }

            PrintController printController = PrintController;
            printController.Print(this);
        }

        //
        // Summary:
        //     Provides information about the print document, in string form.
        //
        // Returns:
        //     A string.
        public override string ToString()
        {
            return "[PrintDocument " + DocumentName + "]";
        }
    }
}
