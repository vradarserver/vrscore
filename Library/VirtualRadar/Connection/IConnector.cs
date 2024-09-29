// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Connection
{
    /// <summary>
    /// The properties and functions that all connectors have in common.
    /// </summary>
    public interface IConnector : IAsyncDisposable
    {
        /// <summary>
        /// Exposes the immutable options that the connector was built with.
        /// </summary>
        IConnectorOptions Options { get; }

        /// <summary>
        /// A terse description of the connection.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The current state of the connection.
        /// </summary>
        ConnectionState ConnectionState { get; }

        /// <summary>
        /// Raised on a random thread when <see cref="ConnectionState"/> changes.
        /// </summary>
        event EventHandler ConnectionStateChanged;

        /// <summary>
        /// The last exception thrown by the connector.
        /// </summary>
        TimestampedException LastException { get; }

        /// <summary>
        /// Raised when <see cref="LastException"/> changes.
        /// </summary>
        event EventHandler LastExceptionChanged;

        /// <summary>
        /// Establishes the connection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ConnectionAlreadyOpenException">
        /// Thrown if an attempt to open a connection is made while the <see cref="ConnectionState"/> is not
        /// <see cref="ConnectionState.Closed"/>.
        /// </exception>
        Task OpenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Closes the connection. The connection should be left in a state where <see cref="OpenAsync"/>
        /// could be called to re-establish the connection. Attempts to close a connection that is not in the
        /// <see cref="ConnectionState.Open"/> state are ignored.
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}
