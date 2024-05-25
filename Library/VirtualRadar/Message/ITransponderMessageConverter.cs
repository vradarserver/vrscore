// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Message
{
    /// <summary>
    /// The interface for objects that can convert to and from message bytes and
    /// a single transponder message.
    /// </summary>
    public interface ITransponderMessageConverter
    {
        /// <summary>
        /// Gets a value indicating that the object can convert a message chunk to a
        /// <see cref="TransponderMessage"/>. This is the internal message format used
        /// everywhere within VRS.
        /// </summary>
        bool CanConvertTo { get; }

        /// <summary>
        /// Gets a value indicating that the object can convert a <see cref="TransponderMessage"/>
        /// to a message chunk.
        /// </summary>
        bool CanConvertFrom { get; }

        /// <summary>
        /// Gets the size of the chunk to allocate for <see cref="ConvertFrom"/> calls. Not used
        /// if <see cref="CanConvertFrom"/> is false.
        /// </summary>
        int AllocateChunkSize { get; }

        /// <summary>
        /// Converts a chunk of memory holding a native feed message into the common <see cref="TransponderMessage"/>.
        /// </summary>
        /// <param name="chunk">The chunk of memory holding the message to convert.</param>
        /// <returns>The transponder message extracted from the chunk or null if it cannot be extracted.</returns>
        TransponderMessage[] ConvertTo(ReadOnlyMemory<byte> chunk);

        /// <summary>
        /// Converts a <see cref="TransponderMessage"/> back into a native message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chunk"></param>
        void ConvertFrom(TransponderMessage message, Memory<byte> chunk);
    }
}
