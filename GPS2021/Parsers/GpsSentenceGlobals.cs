using System;
using System.Globalization;

namespace DonaDona.Device.GPS
{
	#region Gps Latitude/Longitude definitions
	/// <summary>
	/// Parses Latitude/Longitude information from the GPS.
	/// </summary>
	public struct LatitudeLongitude
	{
		public int Degrees;
		public double Minutes;
		public string Indicator;
		public double ddDegrees;

		/// <summary>
		/// written by the Grok AI
		/// </summary>
		/// <param name="degrees"></param>
		/// <param name="minutes"></param>
		/// <param name="seconds"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
        public static double DmsToDecimalDegrees(int degrees, double minutes,  char Indicator = 'N')
        {
            // Validate input
            if (minutes < 0 || minutes >= 60)
                throw new ArgumentException("Minutes must be between 0 and 59");           
            if (Indicator != 'N' && Indicator != 'S' && Indicator != 'E' && Indicator != 'W')
                throw new ArgumentException("Direction must be N, S, E, or W");

			// Calculate decimal degrees
			double decimalDegrees = degrees + (minutes / 60.0);

            // Apply negative sign for South or West directions
            if (Indicator == 'S' || Indicator == 'W')
                decimalDegrees = -decimalDegrees;

            return decimalDegrees;
        }

        public LatitudeLongitude(string LatitudeString, string Indicator)
		{
			string indicator = Indicator.ToUpper();
			CultureInfo cinfo = new CultureInfo("en-US");

			switch(indicator)
			{
				case "E":
				{
						Console.WriteLine("test1");
						Degrees = int.Parse(LatitudeString.Substring(0, 3), cinfo);
						Console.WriteLine("test2");
						Minutes = double.Parse(LatitudeString.Substring(3), cinfo);
						break;
				}
				case "W":
				{
						Console.WriteLine("test3");
						Degrees = int.Parse(LatitudeString.Substring(0, 3), cinfo);
						Console.WriteLine("test4");
						Minutes = double.Parse(LatitudeString.Substring(3), cinfo);
					break;
				}
				case "N":
				{
						Console.WriteLine("test5");
						Degrees = int.Parse(LatitudeString.Substring(0, 2), cinfo);
						Console.WriteLine("test6");
						Minutes = double.Parse(LatitudeString.Substring(2), cinfo);
					break;
				}
				case "S":
				{
					Degrees = int.Parse(LatitudeString.Substring(0, 2), cinfo);
					Minutes = double.Parse(LatitudeString.Substring(2), cinfo);
					break;
				}
				default:
				{
					Degrees = 0;
					Minutes = 0;
					break;
				}
			}
			//Minutes = double.Parse(LatitudeString.Substring(3), new CultureInfo("en-US"));

			ddDegrees = -123;

            this.Indicator = Indicator;

            ///ddDegrees = DmsToDecimalDegrees( Degrees, Minutes, Indicator[0] );

        }
	}
	#endregion

	#region Gps Speed definitions
	/// <summary>
	/// Provides speed information as received from a GPS.
	/// The data is provided in Knots, Km/h and Miles/h.
	/// </summary>
	public struct GpsSpeed
	{
		public double Knots;
		public double KilometersPerHour;
		public double MilesPerHour;

		public GpsSpeed(string ReportedSpeed)
		{
			Knots = 0;
			KilometersPerHour = 0;
			MilesPerHour = 0;

			if(ReportedSpeed.Trim() != "")
			{
				try
				{
					Knots = double.Parse(ReportedSpeed, new CultureInfo("en-US"));
					KilometersPerHour = Knots * 1.85185;
					MilesPerHour = Knots * 1.150779;
				}
				catch(Exception ex)
				{
					// Error parsing speed.
				}
			}
		}
	}
	#endregion

	#region Gps Satellite Information
	/// <summary>
	/// Mantains information aboun a given satellite viewed by the GPS
	/// </summary>
	public struct SatelliteInView
	{
		public int SatelliteID;
		public double Elevation;
		public double Azimuth;
		public double SignalToNoiseRatio;
	}
	#endregion

	#region Enums
	/// <summary>
	/// Determines the position fix provided by the GPS.
	/// </summary>
	public enum PositionFix
	{
		NotAvailableOrInvalid = 0,
		SPSMode = 1,
		DifferentialGPS = 2,
		PPSMode = 3
	}

	/// <summary>
	/// GNSS DOP and Active Satellites Mode 1
	/// </summary>
	public enum Mode1
	{
		Manual = 0,
		Automatic = 1
	}

	/// <summary>
	/// GNSS DOP and Active Satellites Mode 2
	/// </summary>
	public enum Mode2
	{
		FixNotAvailable = 0, 
		TwoDimensions = 1,
		ThreeDimensions = 2
	}
	
	#endregion
}
