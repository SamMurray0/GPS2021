using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS2021
{
    // refer https://docs.novatel.com/OEM7/Content/Logs/GPGLL.htm

    public enum GPStatus
    {
        DataValid=0xa,
        Datainvalid=0 // or any not valid
    }
}
