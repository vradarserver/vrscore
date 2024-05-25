namespace VirtualRadar.Format
{
    public static class GroundSpeed
    {
        public static string IsoRounded(float? knots) => Rounded(knots, CultureInfo.CurrentCulture);

        public static string Rounded(float? knots, IFormatProvider formatProvider) => String.Format(formatProvider, "{0:0.0}", knots);
    }
}
