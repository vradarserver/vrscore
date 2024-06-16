using System.Net;

namespace VirtualRadar.Connection
{
    /// <summary>
    /// Carries options for a <see cref="TcpPullConnector"/>.
    /// </summary>
    /// <param name="Address">
    /// The IP address to connect to. This can be IPv4 or IPv6.
    /// </param>
    /// <param name="Port">
    /// The port to connect to.
    /// </param>
    public record TcpPullConnectorOptions(
        IPAddress Address,
        int Port
    )
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public TcpPullConnectorOptions() : this(IPAddress.None, 0)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Address}:{Port}";
    }
}
