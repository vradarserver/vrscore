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
using System.Buffers.Binary;
using System.IO;

namespace VirtualRadar.Feed.Recording
{
    public class Recorder : IRecorder
    {
        DateTime _StreamStarted;

        public static readonly TimeSpan MaximumRecordingLength = TimeSpan.FromDays(48);

        /// <inheritdoc/>
        public async Task WriteHeaderAsync(Stream stream)
        {
            if(_StreamStarted == default) {
                var header = new Header();
                using(var buffer = MemoryPool<byte>.Shared.Rent(Header.Version1Length)) {
                    Header.MagicNumber.CopyTo(buffer.Memory[0..Header.MagicNumber.Length]);
                    buffer.Memory.Span[Header.MagicNumber.Length] = (byte)header.Version;
                    Encoding
                        .ASCII
                        .GetBytes(header.RecordingStartedUtc.ToString(Header.DateFormat))
                        .CopyTo(buffer.Memory.Span[(Header.MagicNumber.Length + 1)..]);

                    await stream.WriteAsync(buffer.Memory[0..Header.Version1Length]);
                }

                _StreamStarted = header.RecordingStartedUtc;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> WritePacketAsync(Stream stream, ReadOnlyMemory<byte> packet)
        {
            var result = packet.Length == 0;
            if(!result) {
                var durationSinceStart = DateTime.UtcNow - _StreamStarted;
                if(durationSinceStart.TotalMilliseconds < 0) {
                    // Maybe the RTC reset?
                    durationSinceStart = TimeSpan.Zero;
                    _StreamStarted = DateTime.UtcNow;
                }
                result = durationSinceStart <= MaximumRecordingLength;
                if(result) {
                    using(var parcelOwner = FormatParcel(durationSinceStart, packet, out var packetLength)) {
                        await stream.WriteAsync(parcelOwner.Memory[0..packetLength]);
                    }
                }
            }

            return result;
        }

        private static IMemoryOwner<byte> FormatParcel(TimeSpan durationSinceStart, ReadOnlyMemory<byte> packet, out int parcelLength)
        {
            parcelLength = 6 + packet.Length;
            var result = MemoryPool<byte>.Shared.Rent(parcelLength);

            BinaryPrimitives.WriteUInt32BigEndian(result.Memory.Span[0..4], (uint)durationSinceStart.TotalMilliseconds);
            BinaryPrimitives.WriteUInt16BigEndian(result.Memory.Span[4..6], (ushort)packet.Length);
            packet.CopyTo(result.Memory[6..]);

            return result;
        }
    }
}
