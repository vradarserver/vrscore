// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace VirtualRadar.Connection
{
    /// <summary>
    /// An implementation of <see cref="IConnectorDeprecated"/> that can pull a feed from an IPv4/6 address and port.
    /// </summary>
    public class TcpConnector : IConnectorDeprecated
    {
        private Socket _Socket;             // The socket that communication is running over, if this is null then the connection is closed.
        private NetworkStream _Stream;      // The network stream opened on top of the socket, if this is null then the connection is closed.

        /// <summary>
        /// Gets the options that the connector's using.
        /// </summary>
        public TcpConnectorConfig Options { get; }

        /// <inheritdoc/>
        public string Description => $"tcp://{Options.Address}:{Options.Port}";

        private ConnectionState _ConnectionState;
        /// <inheritdoc/>
        public ConnectionState ConnectionState
        {
            get => _ConnectionState;
            set {
                if(value != ConnectionState) {
                    _ConnectionState = value;
                    OnConnectionStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        public bool CanRead => Options.CanRead;

        /// <inheritdoc/>
        public bool CanWrite => Options.CanWrite;

        /// <inheritdoc/>
        public event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected void OnConnectionStateChanged(EventArgs args)
        {
            ConnectionStateChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public TcpConnector(TcpConnectorConfig options)
        {
            if(!options.CanRead && !options.CanWrite) {
                throw new InvalidOperationException("A connector cannot forbid both reading and writing");
            }
            Options = options;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            var exceptions = new List<Exception>();

            try {
                if(_Stream != null) {
                    await _Stream.DisposeAsync();
                }
            } catch(Exception ex) {
                exceptions.Add(ex);
            } finally {
                _Stream = null;
            }

            try {
                if(_Socket != null) {
                    _Socket.Close();
                    _Socket.Dispose();
                }
            } catch(Exception ex) {
                exceptions.Add(ex);
            } finally {
                _Socket = null;
            }

            GC.SuppressFinalize(this);

            if(exceptions.Count != 0) {
                throw new AggregateException(
                    $"Exceptions caught while disposing of a {nameof(TcpConnector)} to {Description}",
                    exceptions
                );
            }
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenAsync(CancellationToken cancellationToken)
        {
            if(ConnectionState != ConnectionState.Closed) {
                throw new ConnectionAlreadyOpenException($"Cannot open a connection that is in the {ConnectionState} state");
            }

            ConnectionState = ConnectionState.Opening;
            try {
                _Socket = new Socket(
                    Options.Address.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

                var ipEndPoint = new IPEndPoint(Options.Address, Options.Port);
                await _Socket.ConnectAsync(ipEndPoint, cancellationToken);

                if(!cancellationToken.IsCancellationRequested) {
                    var access = CanRead && CanWrite
                        ? FileAccess.ReadWrite
                        : CanRead ? FileAccess.Read : FileAccess.Write;

                    _Stream = new NetworkStream(_Socket, access);

                    ConnectionState = ConnectionState.Open;
                }
            } finally {
                if(ConnectionState != ConnectionState.Open) {
                    if(_Stream != null) {
                        try {
                            _Stream.Dispose();
                        } catch {
                            ;       // <-- intentionally swallowing any exceptions here, it's in an unknown state and we already have an exception in flight
                        } finally {
                            _Stream = null;
                        }
                    }
                    if(_Socket != null) {
                        try {
                            _Socket.Dispose();
                        } catch {
                            ;       // <-- intentionally swallowing any exceptions here, it's in an unknown state and we already have an exception in flight
                        } finally {
                            _Socket = null;
                        }
                    }

                    ConnectionState = ConnectionState.Closed;
                }
            }

            return _Stream;
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            if(ConnectionState == ConnectionState.Open) {
                ConnectionState = ConnectionState.Closing;

                try {
                    await _Stream.DisposeAsync();
                    _Stream = null;

                    await _Socket.DisconnectAsync(reuseSocket: false);
                    _Socket.Dispose();
                    _Socket = null;
                } finally {
                    ConnectionState = ConnectionState.Closed;
                }
            }
        }
    }
}
