namespace VirtualRadar.Format
{
    public static class Latitude
    {
        public static string IsoRounded(double? latitude) => Rounded(latitude, CultureInfo.CurrentCulture);

        public static string Rounded(double? latitude, IFormatProvider formatProvider) => String.Format(formatProvider, "{0:0.000000}", latitude);
    }
}
