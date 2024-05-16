using System.Globalization;

namespace VirtualRadar.Format
{
    public static class Track
    {
        public static string IsoRounded(float? degrees) => Rounded(degrees, CultureInfo.CurrentCulture);

        public static string Rounded(float? degrees, IFormatProvider formatProvider) => String.Format(formatProvider, "{0:0}", degrees);
    }
}
