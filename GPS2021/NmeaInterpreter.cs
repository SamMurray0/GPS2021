//*********************************************************************
//**  A high-precision NMEA interpreter
//**  Written by Jon Person, author of "GPS.NET" (www.geoframeworks.com)
//*********************************************************************

using DonaDona.Device.GPS;
using System;
using System.Globalization;
using System.Windows.Documents;

// refer https://github.com/amezcua/GPS-NMEA-Parser/tree/master

namespace GPS2021
{
    public class NmeaInterpreter
    {
        // Represents the EN-US culture, used for numers in NMEA  sentences
        public static CultureInfo NmeaCultureInfo = new CultureInfo("en-US");
        // Used to convert knots into miles per hour
        public static double MPHPerKnot = double.Parse("1.150779",
          NmeaCultureInfo);

        #region Delegates
        public delegate void PositionReceivedEventHandler(string latitude,
          string longitude);
        public delegate void DateTimeChangedEventHandler(System.DateTime dateTime);
        public delegate void BearingReceivedEventHandler(double bearing);
        public delegate void SpeedReceivedEventHandler(double speed);
        public delegate void SpeedLimitReachedEventHandler();
        public delegate void FixObtainedEventHandler();
        public delegate void FixLostEventHandler();
        public delegate void SatelliteReceivedEventHandler(
         int pseudoRandomCode, int azimuth, int elevation, int signalToNoiseRatio);
        public delegate void HDOPReceivedEventHandler(double value);
        public delegate void VDOPReceivedEventHandler(double value);
        public delegate void PDOPReceivedEventHandler(double value);
        #endregion

        #region Events
        public event PositionReceivedEventHandler PositionReceived;
        public event DateTimeChangedEventHandler DateTimeChanged;
        public event BearingReceivedEventHandler BearingReceived;
        public event SpeedReceivedEventHandler SpeedReceived;
        public event SpeedLimitReachedEventHandler SpeedLimitReached;
        public event FixObtainedEventHandler FixObtained;
        public event FixLostEventHandler FixLost;
        public event SatelliteReceivedEventHandler SatelliteReceived;
        public event HDOPReceivedEventHandler HDOPReceived;
        public event VDOPReceivedEventHandler VDOPReceived;
        public event PDOPReceivedEventHandler PDOPReceived;
        #endregion

        public double latitude;
        public double longitude;
        public DateTime GPSTime;
        public GpggaMessage Gpgga = null;
        public GsvParser gsvParser = null;
        public GPVTGGpsSentence gpvtg = null;
        public GPGGAGpsSentence gpgga = null;
        public GPGSVGpsSentence gpgsv = null;

        GPGSVGpsSentence[] gpgsvArray;

        public NmeaInterpreter()
        {
            gpgsvArray = new GPGSVGpsSentence[5];
        }

      




        // Processes information from the GPS receiver
        public bool Parse(string sentence)
        {
            // Discard the sentence if its checksum does not match our 
            // calculated checksum
            if (!IsValid(sentence)) 
                return false;
            // Look at the first word to decide where to go next
            switch (GetWords(sentence)[0])
            {
                case "$GPGSV":
                    gpgsv = new GPGSVGpsSentence(sentence);
                    gpgsvArray[gpgsv.MessageNumber] = gpgsv;
                    break;

                case "$GPGGA":
                    gpgga = new GPGGAGpsSentence(sentence);
                    break;

                case "$GPVTG":
                    gpvtg = new GPVTGGpsSentence(sentence);
                    break;

                case "$GPRMC":
                    // A "Recommended Minimum" sentence was found!

                    GPRMCGpsSentence gpr = new GPRMCGpsSentence(sentence);
                    double latitude = gpr.Longitude.Degrees + gpr.Longitude.Minutes;

                    return ParseGPRMC(sentence);

                //case "$GPGSV":
                //    // A "Satellites in View" sentence was recieved
                //    //return false;                    
                //    return ParseGPGSV(sentence);

                //case "$GPGSV":
                //    if (gsvParser == null)
                //    {
                //        gsvParser = new GsvParser();
                //    }

                //    gsvParser.ParseGsvSentence(sentence);
                //    //Console.WriteLine("SATs 2 =" + gsvParser.output.Count);
                //    return true;


                // needed for the time
                case "$GPGLL":
                    return ParseGPGLL(sentence);                

                case "$GPGSA":
                    GPGSAGpsSentence gsa = new GPGSAGpsSentence(sentence);

                    return ParseGPGSA(sentence);

                //case "$GPGLL":
                //    return ParseGPGLL(sentence);
                //    return;

                //case "$GPGGA":                  
                //    Gpgga = ParseGpgga(sentence);
                //    return true;

                default:
                    // Indicate that the sentence was not recognized
                    Console.WriteLine("No handler for: " + GetWords(sentence)[0] );
                    return false;
            }

            return true;
        }

        // Divides a sentence into individual words
        public string[] GetWords(string sentence)
        {
            return sentence.Split(',' , '*');
        }

        public double ProcessLatitude ( string lat )
        {
            double ans = 0.0;
            Console.WriteLine("test01");
            string tmp = lat.Substring(0, 2);
			Console.WriteLine("test02");
			double latitude = Convert.ToDouble(lat.Substring(0, 2));
			Console.WriteLine("test03");
			tmp = lat.Substring(2);
			Console.WriteLine("test04");
			latitude += Convert.ToDouble(lat.Substring(2)) / 60.0;

            return latitude;
        }

        public double ProcessLongitude(string lng, string W)
        {
			Console.WriteLine("test05");
			string tmp = lng.Substring(0, 3);
			Console.WriteLine("test06");
			double longitude = Convert.ToDouble(lng.Substring(0, 3));
			Console.WriteLine("test07");
			longitude += Convert.ToDouble(lng.Substring(3)) / 60.0;

            if ( W == "W")
            { 
                longitude *= -1;
             }

            return longitude;
        }

        public TimeSpan DecodeTime( string tim )
        {
			Console.WriteLine("test08");
			string tmp = tim.Substring(0, 6);
            DateTime dt = new DateTime();

            DateTime t = DateTime.ParseExact(tmp, "HHmmss", CultureInfo.InvariantCulture);
            TimeSpan time = t - t.Date;

            return time;
        }

        public DateTime DecodeDate( string dat )
        {
            // ti get it to decode me need to add the decade in - lol
            string str = dat.Insert(4, "20");
            DateTime dt = DateTime.ParseExact(str, "ddMMyyyy", CultureInfo.InvariantCulture);

            return dt;
        }

        //public bool ParseGPRMC( string sentence )
        //{
        //    // Divide the sentence into words
        //    string[] Words = GetWords(sentence);

        //    TimeSpan offset;

        //    return true;
        //}

        // TODO this needs decoding next

        //public bool ParseGPGGA( string sentence )
        //{
        //    // Divide the sentence into words
        //    string[] Words = GetWords(sentence);

        //    GpggaMessage gpgga = new GpggaMessage();


        //    return true;
        //}

        private static double ConvertToDecimalDegrees(string coordinate, string direction)
        {
            if (string.IsNullOrEmpty(coordinate))
                return 0.0;

			// Example: 4807.038 = 48 degrees, 07.038 minutes
			Console.WriteLine("test09");
			double degrees = double.Parse(coordinate.Substring(0, coordinate.Length - 7));
			Console.WriteLine("test10");
			double minutes = double.Parse(coordinate.Substring(coordinate.Length - 7)) / 60.0;
            double decimalDegrees = degrees + minutes;

            // Apply negative sign for South or West
            if (direction == "S" || direction == "W")
            {
                decimalDegrees = -decimalDegrees;
            }

            return decimalDegrees;
        }

        public static GpggaMessage ParseGpgga(string nmeaSentence)
        {
            if (string.IsNullOrEmpty(nmeaSentence) || !nmeaSentence.StartsWith("$GPGGA"))
            {
                throw new ArgumentException("Invalid GPGGA sentence");
            }

            // Split the sentence by commas
            var fields = nmeaSentence.Split(',');

            if (fields.Length < 15)
            {
                throw new ArgumentException("Incomplete GPGGA sentence");
            }

            // Extract checksum (after the '*')
            string checksum = fields[fields.Length - 1].Split('*')[1];

            var gpgga = new GpggaMessage
            {
                MessageId = fields[0],
                UtcTime = fields[1],
                Latitude = ConvertToDecimalDegrees(fields[2], fields[3]),
                LatitudeDirection = fields[3],
                Longitude = ConvertToDecimalDegrees(fields[4], fields[5]),
                LongitudeDirection = fields[5],
                FixQuality = int.TryParse(fields[6], out var fix) ? fix : 0,
                NumberOfSatellites = int.TryParse(fields[7], out var sats) ? sats : 0,
                Hdop = double.TryParse(fields[8], out var hdop) ? hdop : 0.0,
                Altitude = double.TryParse(fields[9], out var alt) ? alt : 0.0,
                AltitudeUnit = fields[10],
                GeoidalSeparation = double.TryParse(fields[11], out var geoid) ? geoid : 0.0,
                GeoidalUnit = fields[12],
                AgeOfDgps = fields[13],
                DgpsStationId = fields[14].Split('*')[0], // Remove checksum part
                Checksum = checksum
            };

            return gpgga;
        }

        public bool ParseGPGLL( string sentence )
        {
            // Divide the sentence into words
            string[] Words = GetWords(sentence);

            if (Words[1] != string.Empty && Words[5] != string.Empty)
            {
               
                Console.WriteLine("GPGLL lat:" + Words[1]);
                latitude = ProcessLatitude(Words[1]);
                longitude = ProcessLongitude(Words[3], Words[4]);
                DecodeTime(Words[5]);
            }

            Console.WriteLine("GPGLL lng:" + Words[3] + latitude + longitude);

            return true;
        }

        // Interprets a $GPRMC message
        public bool ParseGPRMC(string sentence)
        {
            // Divide the sentence into words
            string[] Words = GetWords(sentence);
            // Do we have enough values to describe our location?
            if (Words[3] != "" & Words[4] != "" &
              Words[5] != "" & Words[6] != "")
            {
				// Yes. Extract latitude and longitude
				// Append hours
				Console.WriteLine("test11");
				string Latitude = Words[3].Substring(0, 2) + "°";
				Console.WriteLine("test12");
				latitude = Convert.ToDouble(Words[3].Substring(0, 2));
				Console.WriteLine("test13");
				latitude += Convert.ToDouble(Words[3].Substring(2))/60.0;

				// Append minutes
				Console.WriteLine("test14");
				Latitude = Latitude + Words[3].Substring(2) + "\"";
                // Append hours 
                Latitude = Latitude + Words[4]; // Append the hemisphere
				Console.WriteLine("test15");
				string Longitude = Words[5].Substring(0, 3) + "°";
				Console.WriteLine("test16");
				longitude = Convert.ToDouble(Words[5].Substring(0, 3));
				Console.WriteLine("test17");
				longitude += Convert.ToDouble(Words[5].Substring(3)) / 60.0;



				// Append minutes
				Console.WriteLine("test18");
				Longitude = Longitude + Words[5].Substring(3) + "\"";
                // Append the hemisphere
                Longitude = Longitude + Words[6];

                if (Words[6] == "W")
                    longitude *= -1;

                // Notify the calling application of the change
                if (PositionReceived != null)
                    PositionReceived(Latitude, Longitude);
            }

            if (Words[1] != string.Empty && Words[9] != string.Empty )
            {
                TimeSpan ts = DecodeTime(Words[1]);
                DateTime dt = DecodeDate(Words[9]);
                DateTime ActualTime = dt + ts;
                GPSTime = ActualTime;
            }



            //TimeSpan tsz = TimeZone.GetUtcOffset(ActualTime);

            // Do we have enough values to parse satellite-derived time?
            if (Words[1] != "")
            {
				// Yes. Extract hours, minutes, seconds and milliseconds
				Console.WriteLine("test19");
				int UtcHours = Convert.ToInt32(Words[1].Substring(0, 2));
				Console.WriteLine("test20");
				int UtcMinutes = Convert.ToInt32(Words[1].Substring(2, 2));
				Console.WriteLine("test21");
				int UtcSeconds = Convert.ToInt32(Words[1].Substring(4, 2));
                int UtcMilliseconds = 0;
                // Extract milliseconds if it is available
                if (Words[1].Length > 7)
                {
					Console.WriteLine("test22");
					UtcMilliseconds = Convert.ToInt32(Words[1].Substring(7));
                }
                // Now build a DateTime object with all values
                System.DateTime Today = System.DateTime.Now.ToUniversalTime();
                System.DateTime SatelliteTime = new System.DateTime(Today.Year,
                  Today.Month, Today.Day, UtcHours, UtcMinutes, UtcSeconds,
                  UtcMilliseconds);
                // Notify of the new time, adjusted to the local time zone
                if (DateTimeChanged != null)
                    DateTimeChanged(SatelliteTime.ToLocalTime());
            }
            // Do we have enough information to extract the current speed?
            if (Words[7] != "")
            {
                // Yes.  Parse the speed and convert it to MPH
                double Speed = double.Parse(Words[7], NmeaCultureInfo) *
                  MPHPerKnot;
                // Notify of the new speed
                if (SpeedReceived != null)
                    SpeedReceived(Speed);
                // Are we over the highway speed limit?
                if (Speed > 55)
                    if (SpeedLimitReached != null)
                        SpeedLimitReached();
            }
            // Do we have enough information to extract bearing?
            if (Words[8] != "")
            {
                // Indicate that the sentence was recognized
                double Bearing = double.Parse(Words[8], NmeaCultureInfo);
                if (BearingReceived != null)
                    BearingReceived(Bearing);
            }
            // Does the device currently have a satellite fix?
            if (Words[2] != "")
            {
                switch (Words[2])
                {
                    case "A":
                        if (FixObtained != null)
                            FixObtained();
                        break;
                    case "V":
                        if (FixLost != null) FixLost();
                        break;
                }
            }
            // Indicate that the sentence was recognized
            return true;
        }

        // Interprets a "Satellites in View" NMEA sentence
        public bool ParseGPGSV(string sentence)
        {
            int PseudoRandomCode = 0;
            int Azimuth = 0;
            int Elevation = 0; 
            int SignalToNoiseRatio = 0;

            // Divide the sentence into words
            string[] Words = GetWords(sentence);
            // Each sentence contains four blocks of satellite information.  
            // Read each block and report each satellite's information
            int Count = 0;
            for (Count = 1; Count <= 4; Count++)
            {
                // Does the sentence have enough words to analyze?
                if ((Words.Length - 1) >= (Count * 4 + 3))
                {
                    // Yes.  Proceed with analyzing the block.  
                    // Does it contain any information?
                    if (Words[Count * 4] != "" & Words[Count * 4 + 1] != ""
                       & Words[Count * 4 + 2] != "" & Words[Count * 4 + 3] != "")
                    {
                        // Yes. Extract satellite information and report it
                        PseudoRandomCode = System.Convert.ToInt32(Words[Count * 4]);
                        Elevation = Convert.ToInt32(Words[Count * 4 + 1]);
                        Azimuth = Convert.ToInt32(Words[Count * 4 + 2]);

                        string myStr = Words[Count * 4 + 3];

                        //SignalToNoiseRatio = Convert.ToInt32(Words[Count * 4 + 3]);
                        SignalToNoiseRatio = Convert.ToInt32(myStr);

                        // Notify of this satellite's information
                        if (SatelliteReceived != null)
                            SatelliteReceived(PseudoRandomCode, Azimuth,
                            Elevation, SignalToNoiseRatio);
                    }
                }
            }
            // Indicate that the sentence was recognized
            return true;
        }    

        // Interprets a "Fixed Satellites and DOP" NMEA sentence
        public bool ParseGPGSA(string sentence)
        {
            // Divide the sentence into words
            string[] Words = GetWords(sentence);
            // Update the <acronym title="Dilution of Precision">DOP</acronym> values
            if (Words[15] != "")
            {
                if (PDOPReceived != null)
                    PDOPReceived(double.Parse(Words[15], NmeaCultureInfo));
            }
            if (Words[16] != "")
            {
                if (HDOPReceived != null)
                    HDOPReceived(double.Parse(Words[16], NmeaCultureInfo));
            }
            if (Words[17] != "")
            {
                if (VDOPReceived != null)
                    VDOPReceived(double.Parse(Words[17], NmeaCultureInfo));
            }
            return true;
        }

        /// <summary>
        /// string with $ and inc the *
        /// And pass in your string, with $ a the start and * at the end (before the checksum), and commas between words, it works.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        private static string getChecksum(string sentence)
        {
            //Start with first Item
            int checksum = Convert.ToByte(sentence[sentence.IndexOf('$') + 1]);
            // Loop through all chars to get a checksum
            for (int i = sentence.IndexOf('$') + 2; i < sentence.IndexOf('*'); i++)
            {
                // No. XOR the checksum with this character's value
                checksum ^= Convert.ToByte(sentence[i]);
            }
            // Return the checksum formatted as a two-character hexadecimal
            return checksum.ToString("X2");
        }

        // Returns True if a sentence's checksum matches the 
        // calculated checksum
        public bool IsValid(string sentence)
        {
            //return true;

            string check = "*" + getChecksum(sentence);

            if (sentence.Contains(check)) return true;


            //string sub = sentence.Substring(sentence.IndexOf("*") + 1);
            //string check = GetChecksum(sentence);

            return false ;
            // sentence.Substring(sentence.IndexOf("*") + 1) == GetChecksum(sentence);
        }

        // Calculates the checksum for a sentence
        public string GetChecksum(string sentence)
        {
            // Loop through all chars to get a checksum
            int Checksum = 0;
            foreach (char Character in sentence)
            {
                if (Character == '$')
                {
                    // Ignore the dollar sign
                }
                else if (Character == '*')
                {
                    // Stop processing before the asterisk
                    break;
                }
                else
                {
                    // Is this the first value for the checksum?
                    if (Checksum == 0)
                    {
                        // Yes. Set the checksum to the value
                        Checksum = Convert.ToByte(Character);
                    }
                    else
                    {
                        // No. XOR the checksum with this character's value
                        Checksum = Checksum ^ Convert.ToByte(Character);
                    }
                }
            }
            // Return the checksum formatted as a two-character hexadecimal
            return Checksum.ToString("X2");
        }
    } 
}