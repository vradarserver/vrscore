// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using Microsoft.Extensions.Hosting;
using VirtualRadar.CommandLine;
using VirtualRadar.Feed;
using VirtualRadar.Feed.Recording;
using VirtualRadar.IO;

namespace VirtualRadar.Utility.CLIConsole
{
    public class Command_DumpFeed(
        #pragma warning disable IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
        IHost _Host,
        HeaderService _Header,
        IRecordingReader _Reader,
        IFeedFormatFactoryService _FeedFactory
        #pragma warning restore IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
    ) : CommonCommand
    {
        private readonly HexDump _HexDump = new() {
            EmitPartialRows = true,
        };

        public FileInfo LoadFileInfo { get; set; } = null!;

        public bool Show { get; set; }

        public bool ParseMessage { get; set; }

        public string FeedFormat { get; set; } = "";

        public async Task<bool> RunAsync()
        {
            await _Header.OutputCopyrightAsync();
            await _Header.OutputTitleAsync("Dump Feed");
            await _Header.OutputOptionsAsync(
                ("Filename",        LoadFileInfo.ToString()),
                ("Show",            Show.ToString()),
                ("Parse messages",  ParseMessage.ToString()),
                ("Feed format",     FeedFormat)
            );
            await WriteLineAsync();

            _Host.StartVirtualRadarServer();
            try {
                var feedConfig = ParseMessage
                    ? _FeedFactory.GetConfig(FeedFormat)
                    : null;
                var chunker = feedConfig?.CreateChunker();
                IStreamChunkerState? chunkerState = null;

                var countParcels = 0L;

                await WriteLineAsync($"Opening {LoadFileInfo}");
                await using(var stream = new FileStream(LoadFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    await WriteLineAsync($"Initialising feed reader with stream");
                    await _Reader.InitialiseStreamAsync(stream, leaveOpen: true);
                    await WriteLineAsync();

                    Parcel? parcel;
                    do {
                        parcel = await _Reader.GetNextAsync(CancellationToken.None);
                        if(parcel != null && _Reader.Header != null) {
                            if(countParcels++ == 0) {
                                await DumpHeaderAsync(_Reader.Header);
                            }
                            await DumpParcelAsync(_Reader.Header, parcel, countParcels);

                            if(chunker != null) {
                                chunkerState = DumpMessages(chunker, parcel.Packet, chunkerState);
                            }

                            await WriteLineAsync();
                        }
                    } while(parcel != null);
                }
            } finally {
                _Host.StopVirtualRadarServer();
            }

            return true;
        }

        private async Task DumpHeaderAsync(Header header)
        {
            await WriteLineAsync($"HEADER");
            await WriteLineAsync($"------");
            await WriteLineAsync($"Version: {header.Version} ({(header.IsVersionValid ? "valid" : "invalid")})");
            await WriteLineAsync($"Started: {header.RecordingStartedUtc} {(header.RecordingStartedUtc.Kind == DateTimeKind.Utc ? "UTC" : "NOT UTC!")}");
            await WriteLineAsync();

            if(!Show) {
                await WriteLineAsync($"PARCELS");
                await WriteLineAsync($"-------");
            }
        }

        private async Task DumpParcelAsync(Header header, Parcel parcel, long parcelNumber)
        {
            var time = header.RecordingStartedUtc.AddMilliseconds(parcel.MillisecondReceived);

            if(!Show) {
                await WriteLineAsync($"Parcel {parcelNumber,6} packet length {parcel.Packet.Length,6} at offset {parcel.MillisecondReceived} ms {time} UTC");
            } else {
                var heading = $"PARCEL {parcelNumber}";
                await WriteLineAsync(heading);
                await WriteLineAsync(new String('-', heading.Length));
                await WriteLineAsync($"Received:       Offset {parcel.MillisecondReceived} ms ({time} UTC)");
                await WriteLineAsync($"Packet length:  {parcel.Packet.Length} bytes");
                foreach(var line in _HexDump.DumpBuffer(parcel.Packet)) {
                    if(line.StartsWith("        ")) {
                        await WriteLineAsync($"Packet:{line[1..]}");
                    } else {
                        await WriteLineAsync($"      {line}");
                    }
                }
            }
        }

        private IStreamChunkerState DumpMessages(StreamChunker chunker, byte[] packet, IStreamChunkerState? chunkerState)
        {
            void chunkRead(object? _, ReadOnlyMemory<byte> chunk)
            {
                Console.WriteLine();
                Console.WriteLine($"Message {chunker.CountChunksExtracted}");
                foreach(var line in _HexDump.DumpBuffer(chunk)) {
                    Console.WriteLine($"      {line}");
                }
            }

            chunker.ChunkRead += chunkRead;
            try {
                chunkerState = chunker.ParseBlock(packet, chunkerState);
            } finally {
                chunker.ChunkRead -= chunkRead;
            }

            return chunkerState;
        }
    }
}
