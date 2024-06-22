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
    /// A stream that can wrap a recording stream and inject pauses between recorded
    /// playbacks to roughly (very roughly!) reproduce the original stream. The stream
    /// does not support writes or seeks.
    /// </summary>
    public class RecordingPlaybackStream : Stream
    {
        private RecordingReader _Reader;
        private PlaybackTimeSync _PlaybackTime = new();
        private Parcel _CurrentParcel;
        private int _ReturnedUpToOffset;

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => throw new InvalidOperationException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the rate at which packets are played back from the stream. A value
        /// of 1 plays the stream at the speed it was recorded, 2 doubles the playback speed,
        /// 0.5 halves it and so on. A value of zero plays the stream as quickly as it can be
        /// consumed.
        /// </summary>
        public double PlaybackSpeed { get; set; } = 1.0;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="recordingStream"></param>
        /// <param name="leaveOpen"></param>
        public RecordingPlaybackStream(Stream recordingStream, bool leaveOpen)
        {
            var reader = new RecordingReader();
            reader.InitialiseStreamAsync(recordingStream, leaveOpen)
                .GetAwaiter()
                .GetResult();
            _Reader = reader;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _Reader?.TearDownStreamAsync()
                .GetAwaiter()
                .GetResult();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count)
                .GetAwaiter()
                .GetResult();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var result = 0;

            if(_CurrentParcel == null && !_Reader.IsCompleted && !cancellationToken.IsCancellationRequested) {
                _CurrentParcel = await _Reader.GetNextAsync(cancellationToken);
                _ReturnedUpToOffset = 0;

                if(_CurrentParcel != null) {
                    await _PlaybackTime.WaitForEventAsync(_CurrentParcel.MillisecondReceived, cancellationToken);
                }
            }

            if(_CurrentParcel != null && !cancellationToken.IsCancellationRequested) {
                var packetBytesRemaining = _CurrentParcel.Packet.Length - _ReturnedUpToOffset;
                result = Math.Min(packetBytesRemaining, count);
                if(result > 0) {
                    Array.Copy(
                        _CurrentParcel.Packet, _ReturnedUpToOffset,
                        buffer, offset,
                        result
                    );
                    _ReturnedUpToOffset += result;

                    if(_ReturnedUpToOffset == _CurrentParcel.Packet.Length) {
                        _CurrentParcel = null;
                    }
                }
            }

            return result;
        }
    }
}
