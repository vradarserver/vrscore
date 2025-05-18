namespace VirtualRadar.Server.Middleware
{
    static class HttpContextItemKey
    {
        /// <summary>
        /// The value of the item is undocumented. If this key is present then the request is coming from the
        /// Internet.
        /// </summary>
        public const string VrsIsInternet = nameof(VrsIsInternet);      // nothing sets this at the moment, just a placeholder
    }
}
