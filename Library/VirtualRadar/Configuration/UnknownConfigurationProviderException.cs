namespace VirtualRadar.Configuration
{
    [Serializable]
    public class UnknownConfigurationProviderException : Exception
    {
        public UnknownConfigurationProviderException() { }

        public UnknownConfigurationProviderException(string message) : base(message) { }

        public UnknownConfigurationProviderException(string message, Exception inner) : base(message, inner) { }
    }
}
