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
    public class TcpPullConnector : IPullConnector
    {
        /// <summary>
        /// Collects together everything about a connection. Connections are self-consistent, the idea is that
        /// an old connection can tear itself down while the parent connector builds and uses a new one without
        /// the two connections interferring each other by sharing backing fields etc. on the parent connector.
        /// </summary>
        /// <param name="_Connector"></param>
        class Connection(TcpPullConnector _Connector)
        {
            private bool _TearingDown;

            public Socket                   Socket;             // The socket that communication is running over
            public NetworkStream            Stream;             // The network stream that we are reading
            public Thread                   PumpThread;         // The background packet pump thread
            public CancellationTokenSource  PumpCancelToken;    // Internal cancellation token that is never CancellationToken.None
            public CancellationTokenSource  StreamCancelToken;  // Our ThreadCancelToken merged with the cancellation token (which could be None) passed to the OpenAsync call

            public void SetupCancellation(CancellationToken userToken)
            {
                PumpCancelToken = new();
                StreamCancelToken = CancellationTokenSource.CreateLinkedTokenSource(
                    PumpCancelToken.Token,
                    userToken
                );
                StreamCancelToken.Token.Register(TearDown);
            }

            public void TearDown()
            {
                if(!_TearingDown) {
                    _TearingDown = true;

                    try {
                        var exceptions = new List<Exception>();

                        if(PumpCancelToken != null) {
                            exceptions.Capture(() => {
                                if(!PumpCancelToken.IsCancellationRequested) {
                                    PumpCancelToken.Cancel();
                                }
                            });
                            exceptions.Capture(() =>
                                PumpCancelToken.Dispose()
                            );
                            PumpCancelToken = null;
                        }
                        if(StreamCancelToken != null) {
                            exceptions.Capture(() =>
                                StreamCancelToken.Dispose()
                            );
                            StreamCancelToken = null;
                        }
                        if(PumpThread != null) {
                            // Sure would be nice to be able to abort the thread here, but c'est la vie.
                            // All we can do is make sure we don't have a reference to it anywhere. If
                            // cancelling it wasn't enough (and probably isn't, given that it isn't async)
                            // then it should explode once the socket gets whipped away from it.
                            PumpThread = null;
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
                        if(_Connector._Connection == this && _Connector.ConnectionState == ConnectionState.Open) {
                            exceptions.Capture(() =>
                                _Connector.ConnectionState = ConnectionState.Closed     // <-- can throw exceptions via the event handler
                            );
                        }

                        if(exceptions.Count > 0) {
                            throw new ConnectionTearDownException(exceptions);
                        }
                    } finally {
                        _TearingDown = false;
                    }
                }
            }
        }

        private Connection _Connection;     // The current connection

        /// <summary>
        /// Gets the options that the connector's using.
        /// </summary>
        public TcpPullConnectorOptions Options { get; }

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

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public TcpPullConnector(TcpPullConnectorOptions options)
        {
            Options = options;
        }

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

                    if(!connection.StreamCancelToken.IsCancellationRequested) {
                        StartPacketPump(connection);
                        _Connection = connection;
                        ConnectionState = ConnectionState.Open;
                    }
                }
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
        /// Starts a background thread to read the packets off the stream. We use a background thread because
        /// typically there are a bunch of these and they run for the duration of the application. We don't
        /// want to eat into / be at the mercy of the thread pool for these.
        /// </summary>
        /// <param name="connection"></param>
        private void StartPacketPump(Connection connection)
        {
            connection.PumpThread = new Thread(PacketPumpThreadStart);
            connection.PumpThread.Start(connection);
        }

        private void PacketPumpThreadStart(object connectionObj)
        {
            try {
                if(connectionObj is Connection connection) {
                    try {
                        RunPacketPump(connection);
                    } catch(OperationCanceledException) {
                        ;
                    } catch(Exception) {
                        try {
                            // TODO: Expose the exceptions that cause read threads to stop via the connector log
                            connection.TearDown();
                        } catch {
                            ;
                        }
                    }
                }
            } catch {
                // Never let exceptions bubble out of a background thread, it will halt the runtime
                ;
            }
        }

        private void RunPacketPump(Connection connection)
        {
            IMemoryOwner<byte> buffer = null;
            var bufferLength = 0;

            try {
                while(!connection.StreamCancelToken.IsCancellationRequested) {
                    var packetSize = Math.Max(1, Math.Min(PacketSize, 64 * 1024));
                    if(buffer == null || bufferLength != packetSize) {
                        buffer?.Dispose();
                        bufferLength = packetSize;
                        buffer = MemoryPool<byte>.Shared.Rent(bufferLength);
                    }

                    var bytesRead = connection.Stream.Read(buffer.Memory.Span);
                    if(!connection.StreamCancelToken.IsCancellationRequested) {
                        if(bytesRead > 0) {
                            OnPacketReceived(buffer.Memory[..bytesRead]);
                        } else {
                            Thread.Sleep(0);        // Give up timeslice
                        }
                    }
                }
            } finally {
                buffer?.Dispose();
            }
        }
    }
}
