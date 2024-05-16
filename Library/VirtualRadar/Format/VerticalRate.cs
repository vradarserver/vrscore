using System.Globalization;

namespace VirtualRadar.Format
{
    public static class VerticalRate
    {
        public static string IsoRounded(float? verticalRate) => Rounded(verticalRate, CultureInfo.CurrentCulture);

        public static string Rounded(float? verticalRate, IFormatProvider formatProvider) => String.Format(formatProvider, "{0:0}", verticalRate);
    }
}
