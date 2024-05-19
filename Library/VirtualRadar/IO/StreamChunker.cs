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
using VirtualRadar.Buffers;

namespace VirtualRadar.IO
{
    /// <summary>
    /// A base class for objects that read bytes from a stream and separate them into chunks.
    /// </summary>
    /// <remarks><para>
    /// Chunks are discrete messages that we are reading from a stream. They have an identifiable
    /// start and end, which might require some inside-knowledge about the data that we expect to
    /// see on the stream, but generally speaking chunkers do not care about the content of the
    /// chunk. Something else will decode the chunk once the chunker has broken it out of the
    /// stream.
    /// </para>
    /// </remarks>
    public abstract class StreamChunker
    {
        /// <summary>
        /// Used to allocate byte arrays that are exposed to the wider world.
        /// </summary>
        public static IsolatedMemoryPool _IsolatedPool = new();

        /// <summary>
        /// The maximum length of a chunk. If a sequence of bytes has not been identified
        /// as a chunk before this limit is reached then the chunk is abandoned and the
        /// chunker begins looking for the start of the next chunk.
        /// </summary>
        public virtual int MaximumChunkSize { get; } = 100;

        /// <summary>
        /// The number of chunks seen by the chunker over its current run of <see cref="ReadChunksFromStream"/>.
        /// This is reset every time the function is called.
        /// </summary>
        public long CountChunksExtracted { get; private set; }

        /// <summary>
        /// Raised when a chunk is extracted. The chunk is passed as a block of memory. IT
        /// WILL BE RELEASED OR REUSED AS SOON AS THE EVENT HANDLER RETURNS. If you need to
        /// use the chunk after the event handler returns then you must copy it and use the
        /// copy.
        /// </summary>
        public event EventHandler<ReadOnlyMemory<byte>> ChunkRead;

        /// <summary>
        /// Raises <see cref="ChunkRead"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnChunkQueued(ReadOnlyMemory<byte> args)
        {
            ChunkRead?.Invoke(this, args);
        }

        /// <summary>
        /// Reads chunks from the stream forever until the cancellation token is set or it errors
        /// out. Each chunk is exposed via the <see cref="ChunkRead"/> event.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ReadChunksFromStream(Stream stream, CancellationToken cancellationToken)
        {
            using(var readBuffer = MemoryPool<byte>.Shared.Rent(10)) {
                while(!cancellationToken.IsCancellationRequested) {
                    var usableLength = await stream.ReadAsync(readBuffer.Memory, cancellationToken);
                    if(usableLength > 0) {
                        ExtractChunksFromReadBuffer(
                            readBuffer.Memory.Span[..usableLength]
                        );
                    }
                }
            }
        }

        private void ExtractChunksFromReadBuffer(Span<byte> readBuffer)
        {
            var parseable = readBuffer;

            do {
                var startOutcome = FindStartOfChunk(parseable);
                if(startOutcome.StartOffset == -1) {
                    parseable = [];
                } else {
                    var endOutcome = FindEndOfChunk(parseable[startOutcome.StartOffset..]);

                    var chunkLength = endOutcome.EndOffset + 1;
                    using(var chunk = _IsolatedPool.Rent(chunkLength)) {
                        parseable
                            .Slice(startOutcome.StartOffset, chunkLength)
                            .CopyTo(chunk.Memory.Span);
                        OnChunkQueued(chunk.Memory[..chunkLength]);
                    }

                    if(chunkLength <= parseable.Length) {
                        parseable = parseable[(startOutcome.StartOffset + chunkLength)..];
                    }
                }
            } while(parseable.Length > 0);
        }

        /// <summary>
        /// Called at the start of <see cref="ReadChunksFromStream"/> to give derivees the
        /// opportunity to reset state for the start of a fresh stream. This is called after
        /// the base has reset its own state. The base does not need to be called.
        /// </summary>
        protected virtual void ResetStateForNewStream()
        {
            // Do not put code here, it would rely on the derivee calling it.
        }

        /// <summary>
        /// Returns a tuple indicating either the start offset of the chunk in the buffer or the number
        /// of bytes at the start of the buffer that are known to be garbage.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private (int StartOffset, int DiscardLength) FindStartOfChunk(Span<byte> buffer)
        {
            var startOffset = -1;
            var discardLength = 0;

            var window = buffer;

            for(var offset = 0;offset < buffer.Length;++offset) {
                startOffset = FindStartOffsetInWindow(window);
                if(startOffset == -1) {
                    ++discardLength;
                } else {
                    break;
                }
                window = window[1..];
            }

            return (startOffset, discardLength);
        }

        /// <summary>
        /// Returns the offset of the start of the chunk within the window passed across.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        protected abstract int FindStartOffsetInWindow(Span<byte> window);

        /// <summary>
        /// Returns a tuple indicating the offset of the last byte of the chunk or an indication that
        /// the chunk has exceeded the maximum length without the end being seen, and so should be
        /// discarded and a new search started.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private (int EndOffset, int DiscardLength) FindEndOfChunk(Span<byte> buffer)
        {
            var endOffset = -1;
            var discardLength = 0;

            var window = buffer;

            for(var offset = 0;offset < buffer.Length;++offset) {
                endOffset = FindEndOffsetInWindow(window);
                if(endOffset != -1) {
                    break;
                }
                window = window[1..];
            }

            if(endOffset == -1 && buffer.Length >= MaximumChunkSize) {
                discardLength = MaximumChunkSize;
            }

            return (endOffset, discardLength);
        }

        /// <summary>
        /// Returns the offset of the end of the chunk within the window passed across.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        protected abstract int FindEndOffsetInWindow(Span<byte> window);
    }
}
