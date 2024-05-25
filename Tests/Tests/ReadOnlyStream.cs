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

namespace Tests
{
    /// <summary>
    /// A read-only stream that lets you control packet size etc.
    /// </summary>
    public class ReadOnlyStream : Stream
    {
        protected byte[] _BackingStore;

        protected int _PacketSize;
        /// <summary>
        /// Gets the largest blob that a single call to Read will return.
        /// </summary>
        public int PacketSize
        {
            get => _PacketSize;
            set => _PacketSize = Math.Max(1, value);
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _BackingStore.Length;

        private long _Position;
        /// <inheritdoc/>
        public override long Position
        {
            get => _Position;
            set {
                if(value < 0 || value > _BackingStore.Length) {
                    throw new IOException();
                }
                _Position = value;
            }
        }

        /// <summary>
        /// Raised when Read runs out of backing store.
        /// </summary>
        public event EventHandler StreamFinished;

        /// <summary>
        /// Raises <see cref="StreamFinished"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnStreamFinished(EventArgs args) => StreamFinished?.Invoke(this, args);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ReadOnlyStream() : this([], 1)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="content"></param>
        public ReadOnlyStream(byte[] content) : this(content, content.Length)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="packetSize"></param>
        public ReadOnlyStream(
            byte[] content,
            int packetSize
        )
        {
            _BackingStore = content;
            PacketSize = packetSize;
        }

        /// <summary>
        /// Sets properties that are typically of interest to a unit test.
        /// </summary>
        /// <param name="backingStore"></param>
        /// <param name="position"></param>
        /// <param name="packetSize"></param>
        /// <param name="sendOnePacket"></param>
        public void Configure(
            byte[] backingStore = null,
            long position = -1,
            int packetSize = -1,
            bool sendOnePacket = false
        )
        {
            _BackingStore = backingStore ?? _BackingStore;
            _Position = Math.Min(
                position == -1 ? Position : position,
                _BackingStore.Length
            );
            if(sendOnePacket) {
                PacketSize = _BackingStore.Length;
            } else {
                PacketSize = Math.Max(
                    1, Math.Min(
                        packetSize == -1 ? PacketSize : packetSize,
                        _BackingStore.Length
                    )
                );
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            var returnedPacketSize = Math.Min(
                _PacketSize,
                Math.Max(
                    0,
                    Math.Min(count, _BackingStore.Length - Position)
                )
            );

            if(returnedPacketSize == 0) {
                OnStreamFinished(EventArgs.Empty);
            } else {
                Array.Copy(
                    _BackingStore,  Position,
                    buffer,         offset,
                    returnedPacketSize
                );
                Position += returnedPacketSize;
            }

            return (int)returnedPacketSize;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin) {
                case SeekOrigin.Begin:      Position = offset; break;
                case SeekOrigin.Current:    Position += offset; break;
                case SeekOrigin.End:        Position = _BackingStore.Length - offset; break;
                default:                    throw new NotImplementedException();
            }
            return Position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new IOException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new IOException();
    }
}
