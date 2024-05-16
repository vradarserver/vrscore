namespace VirtualRadar.Format
{
    public static class Squawk
    {
        public static string Base10AsBase8(int? squawk) => $"{squawk:0000}";
    }
}
