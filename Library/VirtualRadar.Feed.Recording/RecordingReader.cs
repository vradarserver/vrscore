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
using System.IO.Pipelines;

namespace VirtualRadar.Feed.Recording
{
    /// <inheritdoc/>
    class RecordingReader : IRecordingReader
    {
        private PipeReader  _PipeReader;

        /// <inheritdoc/>
        public bool IsCompleted { get; private set; }

        /// <inheritdoc/>
        public Header Header { get; private set; }

        /// <inheritdoc/>
        public Task InitialiseStreamAsync(Stream stream, bool leaveOpen)
        {
            IsCompleted = false;
            Header = null;
            _PipeReader = PipeReader.Create(
                stream,
                new StreamPipeReaderOptions(leaveOpen: leaveOpen)
            );

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task TearDownStreamAsync()
        {
            if(_PipeReader != null) {
                await _PipeReader.CompleteAsync();
                _PipeReader = null;
            }
        }

        /// <inheritdoc/>
        public void TearDownStream()
        {
            if(_PipeReader != null) {
                _PipeReader.Complete();
                _PipeReader = null;
            }
        }

        /// <inheritdoc/>
        public async Task<Parcel> GetNextAsync(CancellationToken cancellationToken)
        {
            Parcel result = null;

            while(result == null && !IsCompleted && _PipeReader != null && !cancellationToken.IsCancellationRequested) {
                var readResult = await _PipeReader.ReadAsync(cancellationToken);
                IsCompleted = readResult.IsCompleted;

                if(!readResult.IsCanceled) {
                    var buffer = readResult.Buffer;

                    if(Header == null && TryReadHeader(ref buffer, out var header)) {
                        Header = header;
                    }

                    if(Header != null) {
                        if(TryReadParcel(ref buffer, out var parcel)) {
                            result = parcel;
                            buffer = buffer.Slice(buffer.Start, 0);
                        }
                    }

                    _PipeReader.AdvanceTo(buffer.Start, buffer.End);
                }
            }

            return result;
        }

        private static bool TryReadHeader(ref ReadOnlySequence<byte> buffer, out Header header)
        {
            header = null;

            if(buffer.Length >= Header.Version1Length) {
                var v1header = buffer.Slice(0, Header.Version1Length);
                buffer = buffer.Slice(v1header.Length);

                if(Header.MagicNumber.SequenceEqual(v1header.Slice(0, Header.MagicNumber.Length).ToArray())) {
                    var version = v1header.Slice(Header.MagicNumber.Length, 1).ToArray()[0];
                    var dateString = Encoding.ASCII.GetString(v1header.Slice(Header.MagicNumber.Length + 1));
                    if(DateTime.TryParseExact(dateString, Header.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var recordingStartedUtc)) {
                        header = new() {
                            Version =               version,
                            RecordingStartedUtc =   DateTime.SpecifyKind(recordingStartedUtc, DateTimeKind.Utc),
                        };
                    }
                }

                if(header == null || !header.IsVersionValid) {
                    throw new BadFeedHeaderException();
                }
            }

            return header != null;
        }

        private static bool TryReadParcel(ref ReadOnlySequence<byte> buffer, out Parcel parcel)
        {
            parcel = null;

            if(buffer.Length >= Parcel.MinimumLength) {
                var millisecondsOffset = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(0, 4).ToArray());
                var packetLength = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(4, 2).ToArray());
                var parcelLength = 6 + packetLength;
                if(buffer.Length >= parcelLength) {
                    var packet = buffer.Slice(6, packetLength).ToArray();
                    buffer = buffer.Slice(parcelLength);
                    parcel = new() {
                        MillisecondReceived = millisecondsOffset,
                        Packet = packet,
                    };
                }
            }

            return parcel != null;
        }
    }
}
