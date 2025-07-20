using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS2021
{
    public class GpggaMessage
    {
        public string MessageId { get; set; }
        public string UtcTime { get; set; }
        public double Latitude { get; set; }
        public string LatitudeDirection { get; set; }
        public double Longitude { get; set; }
        public string LongitudeDirection { get; set; }
        public int FixQuality { get; set; }
        public int NumberOfSatellites { get; set; }
        public double Hdop { get; set; }
        public double Altitude { get; set; }
        public string AltitudeUnit { get; set; }
        public double GeoidalSeparation { get; set; }
        public string GeoidalUnit { get; set; }
        public string AgeOfDgps { get; set; }
        public string DgpsStationId { get; set; }
        public string Checksum { get; set; }

        public override string ToString()
        {
            return $"GPGGA Message:\n" +
                   $"Time: {UtcTime} UTC\n" +
                   $"Latitude: {Latitude:F6}° {LatitudeDirection}\n" +
                   $"Longitude: {Longitude:F6}° {LongitudeDirection}\n" +
                   $"Fix Quality: {FixQuality}\n" +
                   $"Satellites: {NumberOfSatellites}\n" +
                   $"HDOP: {Hdop}\n" +
                   $"Altitude: {Altitude} {AltitudeUnit}\n" +
                   $"Geoidal Separation: {GeoidalSeparation} {GeoidalUnit}\n" +
                   $"DGPS Age: {AgeOfDgps}\n" +
                   $"DGPS Station ID: {DgpsStationId}\n" +
                   $"Checksum: {Checksum}";
        }
    }
}
