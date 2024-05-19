// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Tests.Mocks;
using VirtualRadar.IO;

namespace Tests.VirtualRadar.IO
{
    [TestClass]
    public class StreamChunker_Tests
    {
        class TestChunker : StreamChunker
        {
            private readonly byte[][] _StartMarkers;

            private readonly byte[][] _EndMarkers;

            private readonly bool _ReadAhead;

            public override int MaximumChunkSize { get; }

            public TestChunker(byte[][] startMarkers, byte[][] endMarkers, int maximumChunkSize, bool readAhead)
            {
                _StartMarkers = startMarkers;
                _EndMarkers = endMarkers;
                _ReadAhead = readAhead;
                MaximumChunkSize = maximumChunkSize;
            }

            protected override int FindEndOffsetInWindow(Span<byte> window) => FindMarkerInWindow(_EndMarkers, window);

            protected override int FindStartOffsetInWindow(Span<byte> window) => FindMarkerInWindow(_StartMarkers, window);

            private int FindMarkerInWindow(byte[][] markers, Span<byte> window)
            {
                var result = -1;

                for(var markerIdx = 0;markerIdx < markers.Length;++markerIdx) {
                    var marker = markers[markerIdx];
                    if(marker.Length == 0) {
                        result = 0;
                    } else {
                        var matchedToIdx = 0;
                        for(var windowIdx = 0;windowIdx < window.Length;++windowIdx) {
                            matchedToIdx = window[windowIdx] == marker[matchedToIdx]
                                ? matchedToIdx + 1
                                : 0;
                            if(matchedToIdx == 0 && !_ReadAhead) {
                                break;
                            }
                            if(matchedToIdx == marker.Length) {
                                result = windowIdx - (matchedToIdx - 1);
                                break;
                            }
                        }
                    }
                }

                return result;
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
            byte[][] startMarkers = null,
            byte[][] endMarkers = null,
            int maxChunkSize = 10,
            bool readAhead = true,
            bool setFields = true,
            Action<ReadOnlyMemory<byte>> callback = null
        )
        {
            var result = new TestChunker(
                startMarkers ?? [ [ 0x00, ], ],
                endMarkers   ?? [ [ 0xff, ], ],
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

        private void AssertChunk(ReadOnlySpan<byte> expected, ReadOnlyMemory<byte> actual, string message = null)
        {
            var areEqual = expected.SequenceEqual(actual.Span);
            if(!areEqual && message == null) {
                message = $"Expected to see [{String.Join(", ", expected.ToArray().Select(r => r.ToString("X2")))}] " +
                          $"actual was [{String.Join(", ", actual.ToArray().Select(r => r.ToString("X2")))}]";
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
    }
}
