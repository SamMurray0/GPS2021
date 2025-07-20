using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS2021
{
    public class GsvParser
    {
        public List<GsvSatellite> output;

        public void ParseGsvSentence(string sentence)
        {
            var satellites = new List<GsvSatellite>();

            // Validate input
            if (string.IsNullOrEmpty(sentence) || !sentence.StartsWith("$"))
                output =  satellites;

            // Split sentence and remove checksum (*XX)
            string[] fields = sentence.Split('*')[0].Split(',');
            if (!fields[0].Contains("GSV"))
                output = satellites;

            // Validate checksum (optional)
            if (!ValidateChecksum(sentence))
            {
                Console.WriteLine("Invalid checksum for sentence: " + sentence);
                output = satellites;
            }

            // Parse GSV fields
            try
            {
                int totalMessages = int.Parse(fields[1]); // Total number of messages
                int messageNumber = int.Parse(fields[2]); // Current message number
                int totalSatellites = int.Parse(fields[3]); // Total satellites in view

                // Each satellite takes 4 fields (PRN, elevation, azimuth, SNR)
                for (int i = 4; i < fields.Length - 1; i += 4)
                {
                    if (i + 3 < fields.Length) // Ensure enough fields for a satellite
                    {
                        var satellite = new GsvSatellite
                        {
                            Prn = string.IsNullOrEmpty(fields[i]) ? 0 : int.Parse(fields[i]),
                            Elevation = string.IsNullOrEmpty(fields[i + 1]) ? 0 : int.Parse(fields[i + 1]),
                            Azimuth = string.IsNullOrEmpty(fields[i + 2]) ? 0 : int.Parse(fields[i + 2]),
                            Snr = string.IsNullOrEmpty(fields[i + 3]) ? 0 : int.Parse(fields[i + 3])
                        };
                        satellites.Add(satellite);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing GSV sentence: {ex.Message}");
            }

            output = satellites;
            Console.WriteLine( "SATS (Start)= " + output.Count() );
           // return satellites;
        }

        private bool ValidateChecksum(string sentence)
        {
            if (!sentence.Contains("*"))
                return false;

            string[] parts = sentence.Split('*');
            if (parts.Length != 2)
                return false;

            string data = parts[0].Substring(1); // Remove '$'
            string checksum = parts[1].Trim();

            int calculatedChecksum = 0;
            foreach (char c in data)
            {
                calculatedChecksum ^= c;
            }

            return calculatedChecksum.ToString("X2").Equals(checksum, StringComparison.OrdinalIgnoreCase);
        }
    }

}
