using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS2021
{
    // refer https://receiverhelp.trimble.com/alloy-gnss/en-us/NMEA-0183messages_GGA.html

    public enum GPSQualityIndicator
    {
        FixNotValid=0,
        GPSFix=1,
        DifferentialGPS=2,
        NotApplicable=3,
        RTKFixed=4,
        RTKFloat=5,
        INSDeadReakoning=6
    }
}
