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
    public class JsonTextChunker_Tests
    {
        private JsonTextChunker _Chunker;

        [TestInitialize]
        public void TestInitialise()
        {
            _Chunker = new();
        }

        private (IStreamChunkerState State, IList<string> Chunks) ParseText(string text, IStreamChunkerState state = null)
        {
            var chunker = _Chunker;
            var encoding = Encoding.UTF8;

            List<string> chunks = [];
            void chunkRead(object sender, ReadOnlyMemory<byte> chunkBytes)
            {
                chunks.Add(encoding.GetString(chunkBytes.Span));
            }

            chunker.ChunkRead += chunkRead;
            try {
                var buffer = encoding.GetBytes(text);
                var newState = chunker.ParseBlock(buffer, state);

                return (State: newState, Chunks: chunks);
            } finally {
                chunker.ChunkRead -= chunkRead;
            }
        }

        [TestMethod]
        public void ParseBlock_Can_Parse_Simple_Json_Object()
        {
            var actual = ParseText(@"{ ""A"": 1 }").Chunks;

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(@"{ ""A"": 1 }", actual[0]);
        }

        [TestMethod]
        public void ParseBlock_Can_Parse_Two_Simple_Json_Objects()
        {
            var actual = ParseText(@"{ ""A"": 1 }{ ""B"": 2 }").Chunks;

            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(@"{ ""A"": 1 }", actual[0]);
            Assert.AreEqual(@"{ ""B"": 2 }", actual[1]);
        }

        [TestMethod]
        public void ParseBlock_Skips_Over_Leading_Garbage()
        {
            var actual = ParseText(@" { ""A"": 1 }").Chunks;

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(@"{ ""A"": 1 }", actual[0]);
        }

        [TestMethod]
        public void ParseBlock_Ignores_Chunk_With_No_Start_Block()
        {
            var actual = ParseText(@"}").Chunks;

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void ParseBlock_Ignores_Chunk_With_No_End_Block()
        {
            var actual = ParseText(@"{").Chunks;

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void ParseBlock_Ignores_Chunk_With_End_Block_Before_Start_Block()
        {
            var actual = ParseText(@"}{").Chunks;

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void ParseBlock_Copes_If_Chunk_Spread_Over_Multiple_Blocks()
        {
            var chunk1 = ParseText(@"{ ""A"":");
            var chunk2 = ParseText(@" 1 }", chunk1.State);

            Assert.AreEqual(0, chunk1.Chunks.Count);
            Assert.AreEqual(1, chunk2.Chunks.Count);
            Assert.AreEqual(@"{ ""A"": 1 }", chunk2.Chunks[0]);
        }

        [TestMethod]
        public void ParseBlock_Ignores_Braces_In_Strings()
        {
            var actual = ParseText(@"{ ""A"": ""{}"" }").Chunks;

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(@"{ ""A"": ""{}"" }", actual[0]);
        }

        [TestMethod]
        public void ParseBlock_Ignores_Escaped_Double_Quotes()
        {
            var actual = ParseText(@"{ ""A"": ""\""{}"" }").Chunks;

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(@"{ ""A"": ""\""{}"" }", actual[0]);
        }

        [TestMethod]
        public void ParseBlock_Returns_Inner_Objects()
        {
            var actual = ParseText(@"{ ""Inner"": { ""Id"": 1 } }").Chunks;

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(@"{ ""Inner"": { ""Id"": 1 } }", actual[0]);
        }
    }
}
