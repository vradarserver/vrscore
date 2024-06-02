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

namespace VirtualRadar.Feed.Recording
{
    /// <summary>
    /// The interface for an object that can read a feed recording.
    /// </summary>
    public interface IRecordingReader
    {
        /// <summary>
        /// True if the stream has finished.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// The header read from the stream.
        /// </summary>
        Header Header { get; }

        /// <summary>
        /// Initialises the stream that we will be reading from.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="leaveOpen"></param>
        Task InitialiseStream(Stream stream, bool leaveOpen);

        /// <summary>
        /// Stops all activity on the stream. Further calls to <see cref="TryGetNext"/> always
        /// return false. You should always call this if you have previously successfully
        /// called <see cref="InitialiseStream"/>. Do not call this if <see cref="TryGetNext"/>
        /// is running.
        /// </summary>
        Task TearDownStream();

        /// <summary>
        /// Fetches the next parcel from the stream.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The next parcel on the stream or null if there are no more parcels.</returns>
        /// <exception cref="BadFeedHeaderException">Thrown if the stream is not a
        /// feed recording.</exception>
        Task<Parcel> GetNext(CancellationToken token);
    }
}
