using System.Net;

namespace VirtualRadar.Connection
{
    /// <summary>
    /// Carries options for a <see cref="TcpConnector"/>.
    /// </summary>
    /// <param name="Address">
    /// The IP address to connect to. This can be IPv4 or IPv6.
    /// </param>
    /// <param name="Port">
    /// The port to connect to.
    /// </param>
    /// <param name="CanRead">
    /// True if the port is opened for reading.
    /// </param>
    /// <param name="CanWrite">
    /// True if the port is opened for writing.
    /// </param>
    public record TcpConnectorOptions(
        IPAddress Address,
        int Port,
        bool CanRead,
        bool CanWrite
    )
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public TcpConnectorOptions() : this(IPAddress.None, 0, false, false)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Address}:{Port} {(CanRead && CanWrite ? "RW" : CanRead ? "RO" : "WO")}";
    }
}
