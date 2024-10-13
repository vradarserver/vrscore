// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.IO
{
    /// <summary>
    /// A stream chunker that extracts a single JSON object from the stream.
    /// </summary>
    public class JsonTextChunker : StreamChunker
    {
        /// <inheritdoc/>
        protected override int _MaximumChunkSize => 1024 * 1024;

        /// <inheritdoc/>
        protected override (int StartOffset, int EndOffset) FindStartAndEndOffset(Span<byte> buffer, int newBlockStartOffset)
        {
            var startOffset = -1;
            var endOffset = -1;

            var inString = false;
            var inEscape = false;
            var braceLevel = -1;
            for(var idx = 0;idx < buffer.Length;++idx) {
                if(inEscape) {
                    inEscape = false;
                    continue;
                }
                var ch = (char)buffer[idx];
                switch(ch) {
                    case '"':
                        inString = !inString;
                        break;
                    case '\\':
                        if(inString) {
                            inEscape = true;
                        }
                        break;
                    case '{':
                        if(!inString && ++braceLevel == 0) {
                            startOffset = idx;
                        }
                        break;
                    case '}':
                        if(!inString && --braceLevel == -1) {
                            endOffset = idx;
                        }
                        break;
                }
                if(endOffset != -1) {
                    break;
                }
            }

            return (startOffset, endOffset);
        }
    }
}
