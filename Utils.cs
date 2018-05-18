namespace BeenThereDoneThat
{
    class Utils
    {
        public static string[] ALTITUDE_UNITS = new string[] { "m", "K", "M", "G", "T", "P", "E", "Z", "Y", "X" };

        public static string FormatAltitude(double altitude)
        {
            int alt = (int)altitude;
            foreach (string unit in ALTITUDE_UNITS)
            {
                if (alt < 1000)
                {
                    return string.Format("{0}{1}", alt, unit);
                }
                alt /= 1000;
            }
            return "Pretty high";
        }
    }
}
