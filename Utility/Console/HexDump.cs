// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Extensions;

namespace VirtualRadar.Utility.CLIConsole
{
    /// <summary>
    /// Formats byte arrays for dumping to screen.
    /// </summary>
    class HexDump
    {
        private int _DumpBufferCount = 0;
        private readonly StringBuilder _DumpBuffer = new();
        private readonly StringBuilder _DecodeBuffer = new();

        /// <summary>
        /// The offset to show before the bytes. If this is zero and <see cref="EmitHeader"/>
        /// is set then the first line returned will be a byte number header.
        /// </summary>
        public long RowOffset { get; set; }

        /// <summary>
        /// Controls whether a byte number header is emitted when <see cref="RowOffset"/> is zero.
        /// </summary>
        public bool EmitHeader { get; set; } = true;

        /// <summary>
        /// Controls whether <see cref="RowOffset"/> is shown as an 8 digit hex number at the start
        /// of each line.
        /// </summary>
        public bool EmitOffset { get; set; } = true;

        /// <summary>
        /// Controls whether a decoded ASCII dump of characters is shown at the end of each line.
        /// </summary>
        public bool EmitDecode { get; set; } = true;

        /// <summary>
        /// The number of bytes to show on each line.
        /// </summary>
        public int RowLength { get; set; } = 16;

        /// <summary>
        /// True to emit rows for the entire buffer, even if it is not a multiple of <see cref="RowLength"/>.
        /// Partial rows reset the offset to zero after each dump.
        /// </summary>
        public bool EmitPartialRows { get; set; }

        public IEnumerable<string> DumpBuffer(
            ReadOnlyMemory<byte> buffer
        )
        {
            for(var idx = 0;idx < buffer.Length;++idx) {
                var b = buffer.Span[idx];
                if(_DumpBuffer.Length != 0) {
                    _DumpBuffer.Append(' ');
                }
                _DumpBuffer.Append(b.ToString("X2"));

                if(EmitDecode) {
                    _DecodeBuffer.Append(b >= 32 && b <= 127 ? (char)b : '.');
                }

                if(++_DumpBufferCount == RowLength) {
                    if(EmitHeader && RowOffset == 0) {
                        yield return FormatHeader();
                    }
                    yield return FormatRow();
                    ClearBuffers();
                    RowOffset += RowLength;
                }
            }

            if(EmitPartialRows) {
                if(_DumpBufferCount > 0) {
                    yield return FormatRow();
                }
                ClearBuffers();
                RowOffset = 0L;
            }
        }

        private void ClearBuffers()
        {
            _DumpBufferCount = 0;
            _DumpBuffer.Clear();
            _DecodeBuffer.Clear();
        }

        private string FormatHeader()
        {
            var result = new StringBuilder();

            if(EmitOffset) {
                result.Append("          ");
            }
            for(var offset = 0;offset < RowLength;++offset) {
                if(offset != 0) {
                    result.Append(' ');
                }
                result.Append(offset.ToString("X2"));
            }
            if(EmitDecode) {
                result.Append("  ");
                for(var offset = 0;offset < RowLength;++offset) {
                    var ch = (offset % 16).ToString("X1");
                    if(ch == "0" && offset > 0) {
                        ch = ".";
                    }
                    result.Append(ch);
                }
            }

            return result.ToString();
        }

        private string FormatRow()
        {
            var result = new StringBuilder();

            if(EmitOffset) {
                result.Append(RowOffset.ToString("X8"));
                result.Append("  ");
            }
            result.Append(_DumpBuffer);

            if(EmitDecode) {
                for(var pad = _DumpBufferCount;pad < RowLength;++pad) {
                    if(pad != 0) {
                        result.Append(' ');
                    }
                    result.Append("  ");
                }
                result.Append("  ");
                result.Append(_DecodeBuffer);
            }

            return result.ToString();
        }
    }
}
