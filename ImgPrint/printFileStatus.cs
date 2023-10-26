using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgPrint
{
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
