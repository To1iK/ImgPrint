using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgPrint
{
    public class printFile : IDisposable
    {
        public string filePath;
        public printFileStatus status;
        public Bitmap bmp1b;

        public printFile(string filePath, printFileStatus status)
        {
            this.filePath = filePath;
            this.status = status;
        }
        public printFile(string filePath)
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
}
