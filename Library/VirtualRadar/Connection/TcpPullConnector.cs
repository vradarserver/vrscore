// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Sockets;
using VirtualRadar.Extensions;

namespace VirtualRadar.Connection
{
    /// <summary>
    /// A connector that actively connects to a remote TCP port and pulls a feed from it.
    /// </summary>
    [Lifetime(Lifetime.Transient)]
    class TcpPullConnector : IPullConnector, IOneTimeConfigurable<TcpPullConnectorSettings>
    {
        /// <summary>
        /// Collects together everything about a connection. Connections are self-consistent, the idea is that
        /// an old connection can tear itself down while the parent connector builds and uses a new one without
        /// the two connections interferring each other by sharing backing fields etc. on the parent connector.
        /// </summary>
        /// <param name="_Connector"></param>
        class Connection(TcpPullConnector _Connector) : CancellableState
        {
            public Socket           Socket;     // The socket that communication is running over
            public NetworkStream    Stream;     // The network stream that we are reading
            public Task             PumpTask;   // The background packet pump thread

            protected override void TearDownState()
            {
                var exceptions = new List<Exception>();

                CancelEverything(exceptions);

                if(PumpTask != null) {
                    exceptions.Capture(() => PumpTask.Wait(5000));
                    PumpTask = null;
                }

                if(Stream != null) {
                    exceptions.Capture(() =>
                        Stream.Dispose()
                    );
                    Stream = null;
                }

                if(Socket != null) {
                    exceptions.Capture(() =>
                        Socket.Dispose()
                    );
                    Socket = null;
                }

                TearDownCancellationTokens(exceptions);

                if(_Connector._Connection == this && _Connector.ConnectionState == ConnectionState.Open) {
                    exceptions.Capture(() =>
                        _Connector.ConnectionState = ConnectionState.Closed     // <-- can throw exceptions via the event handler
                    );
                }

                if(exceptions.Count > 0) {
                    throw new ConnectionTearDownException(exceptions);
                }
            }
        }

        private Connection _Connection;     // The current connection
        private readonly OneTimeConfigurableImplementer<TcpPullConnectorSettings> _OneTimeConfig = new(nameof(TcpPullConnector), new());

        /// <inheritdoc/>
        public TcpPullConnectorSettings Options => _OneTimeConfig.Options;

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
        public event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectionStateChanged(EventArgs args)
        {
            ConnectionStateChanged?.Invoke(this, args);
        }

        private TimestampedException _LastException;
        /// <inheritdoc/>
        public TimestampedException LastException
        {
            get => _LastException;
            private set {
                if(value != LastException) {
                    _LastException = value;
                    OnLastExceptionChanged(EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler LastExceptionChanged;

        /// <summary>
        /// Raises <see cref="LastExceptionChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLastExceptionChanged(EventArgs args)
        {
            LastExceptionChanged?.Invoke(this, args);
        }

        /// <inheritdoc/>
        public int PacketSize { get; set; } = 1024;

        /// <inheritdoc/>
        public event EventHandler<ReadOnlyMemory<byte>> PacketReceived;

        /// <summary>
        /// Raises <see cref="PacketReceived"/>.
        /// </summary>
        /// <param name="packet"></param>
        protected virtual void OnPacketReceived(ReadOnlyMemory<byte> packet)
        {
            PacketReceived?.Invoke(this, packet);
        }

        /// <inheritdoc/>
        public void Configure(TcpPullConnectorSettings options) => _OneTimeConfig.Configure(options);

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            try {
                _Connection?.TearDown();
            } catch(ConnectionTearDownException) {
                // If we're bailing then we don't care about errors during teardown
                ;
            }
            _Connection = null;

            GC.SuppressFinalize(this);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            _OneTimeConfig.AssertConfigured();

            if(ConnectionState != ConnectionState.Closed) {
                throw new ConnectionAlreadyOpenException($"Cannot open a connection that is in the {ConnectionState} state");
            }

            ConnectionState = ConnectionState.Opening;

            Connection connection = null;
            try {
                connection = new(this) {
                    Socket = new Socket(
                        Options.Address.AddressFamily,
                        SocketType.Stream,
                        ProtocolType.Tcp
                    )
                };

                var ipEndPoint = new IPEndPoint(Options.Address, Options.Port);
                await connection.Socket.ConnectAsync(ipEndPoint, cancellationToken);

                if(!cancellationToken.IsCancellationRequested) {
                    connection.Stream = new NetworkStream(connection.Socket, FileAccess.Read);
                    connection.SetupCancellation(cancellationToken);

                    if(!connection.LinkedCancelToken.IsCancellationRequested) {
                        StartPacketPump(connection);
                        _Connection = connection;
                        ConnectionState = ConnectionState.Open;
                    }
                }
            } catch(Exception ex) {
                LastException = new(ex);
                throw;
            } finally {
                if(ConnectionState != ConnectionState.Open) {
                    try {
                        connection?.TearDown();
                    } finally {
                        ConnectionState = ConnectionState.Closed;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public Task CloseAsync()
        {
            if(ConnectionState == ConnectionState.Open) {
                ConnectionState = ConnectionState.Closing;

                try {
                    _Connection.TearDown();
                } finally {
                    _Connection = null;
                    ConnectionState = ConnectionState.Closed;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts a stream pump on a threadpool thread.
        /// </summary>
        /// <param name="connection"></param>
        /// <remarks>
        /// I had originally use a standard thread here instead of a threadpool thread,
        /// under normal circumstances these pump threads are very long-lived and I didn't
        /// want to waste pooled threads on them. However, it turns out that .NET Core
        /// allocates thousands of thread pool threads instead of 20 per processor, and
        /// that the value can also be configured... and having a pump that fits in with
        /// .NET's parallel processing means you can have a meaningful cancellation token.
        /// </remarks>
        private void StartPacketPump(Connection connection)
        {
            connection.PumpTask = PacketPumpThreadStart(connection);
        }

        /// <summary>
        /// Runs the packet pump for a connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private async Task PacketPumpThreadStart(Connection connection)
        {
            try {
                await RunPacketPump(connection);
            } catch(OperationCanceledException) {
                ;
            } catch(Exception ex) {
                try {
                    LastException = new(ex);
                } catch {
                    ;
                }
                try {
                    connection.TearDown();
                } catch {
                    ;
                }
            }
        }

        private async Task RunPacketPump(Connection connection)
        {
            IMemoryOwner<byte> buffer = null;
            var bufferLength = 0;

            try {
                while(!connection.LinkedCancelToken.IsCancellationRequested) {
                    var packetSize = Math.Max(1, Math.Min(PacketSize, 64 * 1024));
                    if(buffer == null || bufferLength != packetSize) {
                        buffer?.Dispose();
                        bufferLength = packetSize;
                        buffer = MemoryPool<byte>.Shared.Rent(bufferLength);
                    }

                    var bytesRead = await connection.Stream.ReadAsync(buffer.Memory, connection.LinkedCancelToken.Token);
                    if(!connection.LinkedCancelToken.IsCancellationRequested) {
                        if(bytesRead > 0) {
                            OnPacketReceived(buffer.Memory[..bytesRead]);
                        } else {
                            await Task.Delay(0);
                        }
                    }
                }
            } finally {
                buffer?.Dispose();
            }
        }
    }
}
