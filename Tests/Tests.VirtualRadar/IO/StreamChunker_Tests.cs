// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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

            public TestChunker(byte[] startMarker, byte[] endMarker, int maximumChunkSize)
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
        private IDisposable                         _TestChunkerState;
        private Action<ReadOnlyMemory<byte>>        _ChunkExtractedCallback;
        private int                                 _CountChunksSeen;
        private CancellationTokenSource             _CancellationTokenSource;
        private CancellationToken                   _CancellationToken;
        private ReadOnlyStream                      _Stream;

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
            _TestChunkerState = null;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _TestChunkerState?.Dispose();
        }

        private TestChunker CreateTestChunker(
            byte[] startMarkers = null,
            byte[] endMarkers = null,
            int maxChunkSize = 3,
            bool setFields = true,
            Action<ReadOnlyMemory<byte>> callback = null
        )
        {
            var result = new TestChunker(
                startMarkers ?? [ 0x00, ],
                endMarkers   ?? [ 0xff, ],
                maximumChunkSize:   maxChunkSize
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

        private void BreakStreamIntoBlocks(Action<ReadOnlyMemory<byte>> action)
        {
            var buffer = new byte[_Stream.PacketSize];
            int bytesRead;
            do {
                bytesRead = _Stream.Read(buffer);
                if(bytesRead > 0) {
                    action(buffer[..bytesRead]);
                }
            } while(bytesRead > 0);
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
        public void ParseBlock_Exposes_Chunk_From_Single_Block()
        {
            _Stream.Configure([ 0x00, 0xff, ], sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Increments_Count_Of_Chunks()
        {
            _Stream.Configure([ 0x00, 0xff, ], sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(1L, _TestChunker.CountChunksExtracted);
        }

        [TestMethod]
        public void ParseBlock_Discards_Bytes_Before_Start_Marker()
        {
            _Stream.Configure([ 0x01, 0x00, 0xff, ], sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Reads_Up_To_End_Marker()
        {
            _Stream.Configure([ 0x01, 0x00, 0xff, 0x01 ], sendOnePacket: true);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Can_Read_Two_Chunks_From_One_Packet()
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

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(2, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Can_Read_Chunk_Built_From_Many_Packets()
        {
            _Stream.Configure([ 0x00, 0xff, ], packetSize: 1);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Can_Read_Two_Chunks_Built_From_Many_Packets()
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

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(2, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Can_Read_Very_Large_Chunk()
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

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(1, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Abandons_Chunks_That_Are_Too_Long()
        {
            _Stream.Configure([ 0x00, 0x01, 0x02, 0xff, ], sendOnePacket: true);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(0, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Can_Handle_Buffers_That_Exceed_MaxChunkSize()
        {
            var packet = new byte[100 * 1024];
            new Random().NextBytes(packet);
            for(var idx = 0;idx < packet.Length;++idx) {
                switch(packet[idx]) {
                    case 0x00:
                    case 0xff:
                        packet[idx] = 0x80;
                        break;
                }
            }
            packet[0] = 0x00;
            packet[^1] = 0xff;

            var streamContent = new byte[packet.Length + 2];
            packet.AsSpan().CopyTo(
                streamContent.AsSpan(1, packet.Length)
            );

            CreateTestChunker(maxChunkSize: 10);
            _Stream.Configure(packet, sendOnePacket: true);
            _ChunkExtractedCallback = args => AssertChunk(packet, args);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(0, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Can_Read_Two_Chunks_Built_From_Many_Packets_While_Ignoring_OverLength_Chunk()
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

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(2, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Can_Read_Two_Chunks_Built_From_Many_Packets_While_Ignoring_OverLength_And_Unfinished_Chunk()
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

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(2, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Ignores_Large_Blocks_Of_Leading_Garbage()
        {
            var streamContent = new byte[(1024 * 1024) + 2];
            Array.Fill(streamContent, (byte)0x80);
            streamContent[^2] = 0x00;
            streamContent[^1] = 0xff;
            _Stream.Configure(streamContent, packetSize: 16);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0xff, ], chunk);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

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
        public void ParseBlock_Can_Cope_With_Incomplete_Messages_At_End_Of_Packet(int packetSize)
        {
            _Stream.Configure([
                0x00, 0x01, 0xff,   0x00, 0x02, 0xff,   0x00, 0x03, 0xff,
                0x00, 0x04, 0xff,   0x00, 0x05, 0xff,   0x00, 0x06, 0xff,
                0x00, 0x07, 0xff,   0x00, 0x08, 0xff,   0x00, 0x09, 0xff,
            ], packetSize: packetSize);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, (byte)_CountChunksSeen, 0xff ], chunk);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(9, _CountChunksSeen);
        }

        [TestMethod]
        public void ParseBlock_Can_Cope_With_Messages_Spanning_More_Than_One_Packet()
        {
            // This test relies on inside knowledge of .NET 8 and the chunker, I'm not sure
            // how useful it will be in the future. It relies on knowing that the .NET pool
            // will allocate in blocks of 16 bytes
            CreateTestChunker(maxChunkSize: 17);
            _Stream.Configure([
              //   0     1     2     3     4     5     6     7     8     9     a     b     c     d     e     f
                0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00,
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0xff,
                0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
            ], packetSize: 16);
            _ChunkExtractedCallback = chunk => AssertChunk([ 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0xff ], chunk);

            BreakStreamIntoBlocks(buffer =>
                _TestChunkerState = _TestChunker.ParseBlock(buffer, _TestChunkerState)
            );

            Assert.AreEqual(1, _CountChunksSeen);
        }
    }
}
