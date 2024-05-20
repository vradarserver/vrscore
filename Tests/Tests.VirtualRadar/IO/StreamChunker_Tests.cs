// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;
using Tests.Mocks;
using VirtualRadar.IO;

namespace Tests.VirtualRadar.IO
{
    [TestClass]
    public class StreamChunker_Tests
    {
        class TestChunker : StreamChunker
        {
            private readonly byte[] _StartMarker;

            private readonly byte[] _EndMarker;

            protected override int _MaximumChunkSize { get; }

            public TestChunker(byte[] startMarker, byte[] endMarker, int maximumChunkSize, bool readAhead)
            {
                _StartMarker = startMarker;
                _EndMarker = endMarker;
                _MaximumChunkSize = maximumChunkSize;
            }

            protected override (int, int) FindStartAndEndOffset(Span<byte> buffer, int newBlockStartOffset)
            {
                var startOffset = -1;
                var endOffset = -1;

                for(var idx = 0;idx < buffer.Length;++idx) {
                    var window = buffer[idx..];
                    if(window.Length >= _StartMarker.Length && window[.._StartMarker.Length].SequenceEqual(_StartMarker)) {
                        startOffset = idx;
                        break;
                    }
                }

                if(startOffset != -1) {
                    var startSearchOffset = Math.Max(
                        0,
                        newBlockStartOffset - Math.Max(_StartMarker.Length, _EndMarker.Length)
                    );
                    for(var idx = startSearchOffset;idx < buffer.Length;++idx) {
                        var window = buffer[idx..];
                        if(window.Length >= _EndMarker.Length && window[.._EndMarker.Length].SequenceEqual(_EndMarker)) {
                            endOffset = idx;
                            break;
                        }
                    }
                }

                return (startOffset, endOffset);
            }
        }

        private TestChunker                         _TestChunker;
        private Action<ReadOnlyMemory<byte>>        _ChunkExtractedCallback;
        private int                                 _CountChunksSeen;
        private CancellationTokenSource             _CancellationTokenSource;
        private CancellationToken                   _CancellationToken;
        private MockReadOnlyStream                  _Stream;

        [TestInitialize]
        public void TestInitialise()
        {
            _CancellationTokenSource = new();
            _CancellationToken = _CancellationTokenSource.Token;
            _Stream = new();
            _Stream.StreamFinished += (_,_) => _CancellationTokenSource.Cancel();

            _CountChunksSeen = 0;
            _ChunkExtractedCallback = null;
            _TestChunker = CreateTestChunker();
        }

        private TestChunker CreateTestChunker(
            byte[] startMarkers = null,
            byte[] endMarkers = null,
            int maxChunkSize = 3,
            bool readAhead = true,
            bool setFields = true,
            Action<ReadOnlyMemory<byte>> callback = null
        )
        {
            var result = new TestChunker(
                startMarkers ?? [ 0x00, ],
                endMarkers   ?? [ 0xff, ],
                maximumChunkSize:   maxChunkSize,
                readAhead:          readAhead
            );

            callback ??= (args) => _ChunkExtractedCallback?.Invoke(args);
            result.ChunkRead += (_,chunk) => {
                ++_CountChunksSeen;
                callback(chunk);
            };

            if(setFields) {
                _TestChunker = result;
            }

            return result;
        }

        private string FormatChunk(ReadOnlySpan<byte> chunk, int showFirst = 10, int showLast = 2)
        {
            var result = new StringBuilder();

            var array = chunk.ToArray();
            var firstLump = showFirst;
            if(array.Length == showFirst + showLast) {
                firstLump += showLast;
            }
            result.Append(String.Join(", ", array.Take(firstLump).Select(b => b.ToString("X2"))));

            if(array.Length > firstLump) {
                result.Append($" ... +{array.Length - (showFirst + showLast)} ... ");
                result.Append(String.Join(", ", array[^showLast..].Select(b => b.ToString("X2"))));
            }

            return result.ToString();
        }

        private void AssertChunk(ReadOnlySpan<byte> expected, ReadOnlyMemory<byte> actual, string message = null)
        {
            var areEqual = expected.SequenceEqual(actual.Span);
            if(!areEqual && message == null) {
                message = $"Expected to see [{FormatChunk(expected)}] actual was [{FormatChunk(actual.Span)}]";
            }
            Assert.IsTrue(areEqual, message);
        }

        [TestMethod]
        public async Task Read_Exposes_Chunk_From_Stream()
        {
            _Stream.Configure([ 0x00, 0xff, ], sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Can_Expose_Unfiltered_Content()
        {
            var streamContent = new byte[] { 0xa0, 0xa1, 0xa2, 0x00, 0xff, 0xee };
            _Stream.Configure(streamContent, sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            byte[] block = [];
            _TestChunker.BlockRead += (_,a) => block = a.ToArray();

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.IsTrue(block.SequenceEqual(streamContent));
        }

        [TestMethod]
        public async Task Read_Increments_Count_Of_Chunks()
        {
            _Stream.Configure([ 0x00, 0xff, ], sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(1L, _TestChunker.CountChunksExtracted);
        }

        [TestMethod]
        public async Task Read_Discards_Bytes_Before_Start_Marker()
        {
            _Stream.Configure([ 0x01, 0x00, 0xff, ], sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Reads_Up_To_End_Marker()
        {
            _Stream.Configure([ 0x01, 0x00, 0xff, 0x01 ], sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Can_Read_Two_Chunks_From_One_Packet()
        {
            _Stream.Configure([
                0x00, 0x01, 0xff,
                0x00, 0x02, 0xff,
            ], sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk(_CountChunksSeen == 1
                ? [ 0x00, 0x01, 0xff ]
                : [ 0x00, 0x02, 0xff ],
                chunk
            );

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(2, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Can_Read_Chunk_Built_From_Many_Packets()
        {
            _Stream.Configure([ 0x00, 0xff, ], packetSize: 1);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Can_Read_Two_Chunks_Built_From_Many_Packets()
        {
            _Stream.Configure([
                0x00, 0x01, 0xff,
                0x00, 0x02, 0xff,
            ], packetSize: 1);
            _ChunkExtractedCallback = chunk => AssertChunk(_CountChunksSeen == 1
                ? [ 0x00, 0x01, 0xff ]
                : [ 0x00, 0x02, 0xff ],
                chunk
            );

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(2, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Can_Read_Very_Large_Chunk()
        {
            var chunk = new byte[1024 * 1024];
            new Random().NextBytes(chunk);
            for(var idx = 0;idx < chunk.Length;++idx) {
                switch(chunk[idx]) {
                    case 0x00:
                    case 0xff:
                        chunk[idx] = 0x80;
                        break;
                }
            }
            chunk[0] = 0x00;
            chunk[^1] = 0xff;

            var streamContent = new byte[chunk.Length + 2];
            chunk.AsSpan().CopyTo(
                streamContent.AsSpan(1, chunk.Length)
            );

            CreateTestChunker(maxChunkSize: chunk.Length);
            _Stream.Configure(chunk, packetSize: 1024);
            _ChunkExtractedCallback = args => AssertChunk(chunk, args);

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Abandons_Chunks_That_Are_Too_Long()
        {
            _Stream.Configure([ 0x00, 0x01, 0x02, 0xff, ], sendOnePacket: true);

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(0, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Can_Read_Two_Chunks_Built_From_Many_Packets_While_Ignoring_OverLength_Chunk()
        {
            _Stream.Configure([
                0x00, 0x01, 0xff,
                0x00, 0x01, 0x02, 0xff,
                0x00, 0x02, 0xff,
            ], packetSize: 1);
            _ChunkExtractedCallback = chunk => AssertChunk(_CountChunksSeen == 1
                ? [ 0x00, 0x01, 0xff ]
                : [ 0x00, 0x02, 0xff ],
                chunk
            );

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(2, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Can_Read_Two_Chunks_Built_From_Many_Packets_While_Ignoring_OverLength_And_Unfinished_Chunk()
        {
            _Stream.Configure([
                0x00, 0x01, 0xff,
                0x00, 0x01, 0x02, 0x03,
                0x00, 0x02, 0xff,
            ], packetSize: 1);
            _ChunkExtractedCallback = chunk => AssertChunk(_CountChunksSeen == 1
                ? [ 0x00, 0x01, 0xff ]
                : [ 0x00, 0x02, 0xff ],
                chunk
            );

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(2, _CountChunksSeen);
        }

        [TestMethod]
        public async Task Read_Ignores_Large_Blocks_Of_Leading_Garbage()
        {
            var streamContent = new byte[(1024 * 1024) + 2];
            Array.Fill(streamContent, (byte)0x80);
            streamContent[^2] = 0x00;
            streamContent[^1] = 0xff;
            _Stream.Configure(streamContent, packetSize: 16);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(7)]
        [DataRow(8)]
        [DataRow(9)]
        [DataRow(10)]
        public async Task Read_Can_Cope_When_Start_Seen_At_Very_End_Of_First_Read(int packetSize)
        {
            // This test is a bit dodgy - we need to be sure that if we see the start of a packet
            // at the very end of the first read then we will not end up overrunning the buffer that
            // holds incomplete packets. It relies on insider knowledge of the chunker, and might
            // not catch the same issue in the future.
            //
            // Actually this might be a better test that I thought... it's thrown up problems that I
            // wasn't expecting and not reproduced the issue that I was hoping to fix :) Always good
            // to stumble across that kind of thing.
            _Stream.Configure([
                0x00, 0x01, 0xff,   0x00, 0x02, 0xff,   0x00, 0x03, 0xff,
                0x00, 0x04, 0xff,   0x00, 0x05, 0xff,   0x00, 0x06, 0xff,
                0x00, 0x07, 0xff,   0x00, 0x08, 0xff,   0x00, 0x09, 0xff,
            ], packetSize: packetSize);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, (byte)_CountChunksSeen, 0xff ], chunk);

            await _TestChunker.ReadChunksFromStream(_Stream, _CancellationToken);

            Assert.AreEqual(9, _CountChunksSeen);
        }
    }
}
