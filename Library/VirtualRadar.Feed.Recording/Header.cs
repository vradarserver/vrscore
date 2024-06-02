// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Feed.Recording
{
    /// <summary>
    /// Represents a header in a feed recording.
    /// </summary>
    public class Header
    {
        /// <summary>
        /// The length of the Version 1 header.
        /// </summary>
        public static readonly int Version1Length = 23;

        /// <summary>
        /// The magic number placed at the start of a recording.
        /// </summary>
        public static readonly byte[] MagicNumber = Encoding.ASCII.GetBytes("VRSFR");

        /// <summary>
        /// The current version of the recording format.
        /// </summary>
        public static readonly byte CurrentVersion = 0x01;

        /// <summary>
        /// The version number read from the header.
        /// </summary>
        public int Version { get; init; }

        /// <summary>
        /// True if <see cref="Version"/> is valid.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool IsVersionValid => Version > 0 && Version <= CurrentVersion;

        /// <summary>
        /// The date and time that the recording was started at UTC.
        /// </summary>
        public DateTime RecordingStartedUtc { get; init; }

        /// <summary>
        /// The format string used to encode the recording start date in the header.
        /// </summary>
        public static readonly string DateFormat = "yyyyMMddHHmmssfff";

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Header()
        {
            Version = 1;
            RecordingStartedUtc = DateTime.UtcNow;
        }

        /// <inheritdoc/>
        public override string ToString() => $"Version {Version} started {RecordingStartedUtc} UTC";
    }
}
