// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
        /// chunker begins looking for the start of the next chunk. Note that the chunker
        /// will allocate a buffer of this size, so don't set it to anything mad.
        /// </summary>
        protected virtual int _MaximumChunkSize { get; } = 100;

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
        protected virtual void OnChunkRead(ReadOnlyMemory<byte> args)
        {
            ChunkRead?.Invoke(this, args);
        }

        /// <summary>
        /// Raised when a block is read from the stream - i.e. this can be used to monitor
        /// a pass-through of all bytes on the stream. The block is passed as a block of
        /// memory. IT WILL BE RELEASED OR REUSED AS SOON AS THE EVENT HANDLER RETURNS. If
        /// you need to use the block after the event handler returns then you must copy it
        /// and use the copy.
        /// </summary>
        public event EventHandler<ReadOnlyMemory<byte>> BlockRead;

        /// <summary>
        /// Raises <see cref="BlockRead"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnBlockRead(ReadOnlyMemory<byte> buffer, int bufferLength)
        {
            if(BlockRead != null) {
                BlockRead?.Invoke(this, buffer[..bufferLength]);
            }
        }

        /// <summary>
        /// Given a block read as a part of a contiguous feed of blocks, this extracts chunks from the block
        /// and raises <see cref="ChunkRead"/> events for each block.
        /// </summary>
        /// <param name="buffer">The next packet / datagram etc. read from a connection to a feed.</param>
        /// <param name="state">
        /// The state object returned by the previous call to <see cref="ParseBlock"/> for this feed.
        /// </param>
        /// <returns>
        /// A state object that you should pass to the next call to <see cref="ParseBlock"/> for this feed.
        /// The state is disposable, you must dispose of it once the last block has been read or you will leak
        /// memory.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public IStreamChunkerState ParseBlock(ReadOnlyMemory<byte> buffer, IStreamChunkerState state)
        {
            var parseState = state as StreamChunkerParseState;
            if(state != null && parseState == null) {
                throw new ArgumentException(null, nameof(state));
            }
            parseState ??= new();

            var bufferOffset = 0;
            while(bufferOffset < buffer.Length) {
                var parseBufferUsable = _MaximumChunkSize - parseState.ParseBufferLength;
                var windowLength = Math.Min(buffer.Length - bufferOffset, parseBufferUsable);
                if(windowLength < 1) {
                    throw new InvalidOperationException($"Unexpected window length when parsing block: {nameof(parseBufferUsable)}={parseBufferUsable}");
                }
                var bufferEndOffset = bufferOffset + windowLength;
                var bufferWindow = buffer.Span[bufferOffset..bufferEndOffset];

                var newBlockStart = parseState.ParseBufferLength;
                var newParseBufferLength = Math.Min(newBlockStart + bufferWindow.Length, _MaximumChunkSize);
                parseState.ExpandParseBuffer(_IsolatedPool, newParseBufferLength);

                bufferWindow.CopyTo(
                    parseState.ParseBuffer.Memory.Span[parseState.ParseBufferLength..]
                );

                parseState.ParseBufferLength = ExtractChunks(
                    parseState.ParseBuffer.Memory.Span[..(parseState.ParseBufferLength + bufferWindow.Length)],
                    newBlockStart
                );

                bufferOffset += bufferWindow.Length;
            }

            return parseState;
        }

        private int ExtractChunks(Span<byte> buffer, int newBlockStartOffset)
        {
            var window = buffer;
            var moveWindowToStartOfBuffer = false;

            do {
                (var startOffset, var endOffset) = FindStartAndEndOffset(
                    window,
                    newBlockStartOffset
                );

                if(startOffset == -1 && endOffset == -1) {
                    if(window.Length > _MaximumChunkSize) {
                        window = [];
                    }
                    break;
                }
                if(startOffset != -1 && endOffset == -1) {
                    var incompleteChunkSize = window.Length - startOffset;
                    if(incompleteChunkSize >= _MaximumChunkSize) {
                        window = [];
                        break;
                    } else {
                        moveWindowToStartOfBuffer = true;
                        window = window[startOffset..];
                    }
                }
                if(startOffset == -1 || endOffset == -1 || startOffset > endOffset) {
                    break;
                }

                var chunkLength = (endOffset - startOffset) + 1;
                if(chunkLength <= _MaximumChunkSize) {
                    using(var chunk = _IsolatedPool.Rent(chunkLength)) {
                        ++CountChunksExtracted;
                        window
                            .Slice(startOffset, chunkLength)
                            .CopyTo(chunk.Memory.Span);
                        OnChunkRead(chunk.Memory[..chunkLength]);
                    }
                }

                if(chunkLength <= window.Length) {
                    moveWindowToStartOfBuffer = true;
                    window = window[(endOffset + 1)..];
                    newBlockStartOffset = 0;
                }
            } while(window.Length > 0);

            if(moveWindowToStartOfBuffer && window.Length > 0) {
                window.CopyTo(buffer);
            }
            if(window.Length == _MaximumChunkSize) {
                window = [];
            }

            return window.Length;
        }

        /// <summary>
        /// When overridden by the derivee this returns a tuple indicating the start and end offsets
        /// (inclusive) of the chunk within the buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to search within. Do not use this buffer after the function has returned.
        /// </param>
        /// <param name="newBlockStartOffset">
        /// The offset of the new bytes in the buffer. If this is non-zero then the derivee has seen some of
        /// this content before, the new content begins at this offset.
        /// </param>
        /// <returns>
        /// Start and end offsets of the first chunk in the buffer. They will be the same value for a single
        /// byte chunk. If there is more than one chunk in the buffer then only the first chunk is to be
        /// recognised. Pass -1 for offsets that cannot be found.
        /// </returns>
        protected abstract (int StartOffset, int EndOffset) FindStartAndEndOffset(
            Span<byte> buffer,
            int newBlockStartOffset
        );
    }
}
