using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS2021
{
    public class GsvSatellite
    {
        public int Prn { get; set; }           // Satellite PRN number
        public int Elevation { get; set; }     // Elevation in degrees (0-90)
        public int Azimuth { get; set; }       // Azimuth in degrees (0-359)
        public int Snr { get; set; }           // Signal-to-noise ratio in dBHz (0-99, or 0 if not tracking)
    }
}
