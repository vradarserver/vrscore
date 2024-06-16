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
    /// Based on <see cref="AggregateException"/>, this can be thrown when an <see cref="IConnector"/> tears
    /// down a connection.
    /// </summary>
    /// <remarks>
    /// It is common for connections to collapse due to conditions beyond the program's control, and when the
    /// connection is in a collapsed state the framework often throws exceptions when you try to dispose of it.
    /// By having a distinct exception for exceptions thrown when a connection is torn down the application
    /// can discriminate between these exceptions and those that are more likely to indicate a problem with the
    /// program or setup.
    /// </remarks>
    [Serializable]
    public class ConnectionTearDownException : AggregateException
    {
        public ConnectionTearDownException()
        {
        }

        public ConnectionTearDownException(IEnumerable<Exception> innerExceptions) : base(innerExceptions)
        {
        }

        public ConnectionTearDownException(params Exception[] innerExceptions) : base(innerExceptions)
        {
        }

        public ConnectionTearDownException(string message) : base(message)
        {
        }

        public ConnectionTearDownException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions)
        {
        }

        public ConnectionTearDownException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConnectionTearDownException(string message, params Exception[] innerExceptions) : base(message, innerExceptions)
        {
        }
    }
}
