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

namespace VirtualRadar.Utility.CLIConsole
{
    class StreamDumperService
    {
        public async Task DumpStream(Stream stream, bool showHex, string saveToFileName, CancellationToken cancellationToken)
        {
            var hexDump = new HexDump() {
                EmitHeader = false,
            };

            var buffer = new byte[16];
            var bufferOffset = 0;

            using(var saveToFileStream = await OpenSaveToFileStream(saveToFileName)) {
                while(!cancellationToken.IsCancellationRequested) {
                    var read = await stream.ReadAsync(buffer, bufferOffset, buffer.Length - bufferOffset);
                    if(read > 0) {
                        saveToFileStream.Write(buffer, bufferOffset, read);

                        if(showHex) {
                            foreach(var line in hexDump.DumpBuffer(buffer.AsMemory(0, read))) {
                                await Console.Out.WriteLineAsync(line);
                            }
                        }
                    }
                }

                await saveToFileStream.FlushAsync();
                saveToFileStream.Close();
            }
        }

        private async Task<Stream> OpenSaveToFileStream(string saveToFileName)
        {
            FileStream result = null;

            if(!String.IsNullOrEmpty(saveToFileName)) {
                var folder = Path.GetDirectoryName(saveToFileName);
                if(folder != "" && !Directory.Exists(folder)) {
                    await Console.Out.WriteLineAsync("Creating directory {folder}");
                    Directory.CreateDirectory(folder);
                }

                result = new FileStream(saveToFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            }

            return result ?? Stream.Null;
        }
    }
}
