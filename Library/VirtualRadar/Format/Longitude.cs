namespace VirtualRadar.Format
{
    public static class Longitude       // <-- this should be inheriting from Latitude as they are 100% the same, but .NET
                                        // *barely* has inheritence at the best of times and just craps the bed when it
                                        // comes to static inheritence. I daresay there's a great compiler reason for not
                                        // having it, but I don't care what it is. I'm not writing a compiler.
                                        //
                                        // Anyway.
                                        //
                                        // Here's Wonderwall.
    {
        public static string IsoRounded(double? latitude) => Latitude.IsoRounded(latitude);

        public static string Rounded(double? latitude, IFormatProvider formatProvider) => Latitude.Rounded(latitude, formatProvider);
    }
}
