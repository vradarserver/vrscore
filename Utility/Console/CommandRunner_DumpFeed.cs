﻿// Copyright © 2024 onwards, Andrew Whewell
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
using VirtualRadar.Feed;
using VirtualRadar.Feed.Recording;
using VirtualRadar.IO;

namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_DumpFeed(
        Options _Options,
        HeaderService _Header,
        IRecordingReader _Reader,
        IFeedFormatFactoryService _FeedFactory,
        IHost _Host
    ) : CommandRunner
    {
        private readonly HexDump _HexDump = new() {
            EmitPartialRows = true,
        };

        public override async Task<bool> Run()
        {
            await _Header.OutputCopyright();
            await _Header.OutputTitle("Dump Feed");
            await _Header.OutputOptions(
                ("Filename",        _Options.LoadFileName),
                ("Show",            _Options.Show.ToString()),
                ("Parse messages",  _Options.ParseMessage.ToString()),
                ("Feed format",     _Options.FeedFormat)
            );
            await WriteLine();

            if(!File.Exists(_Options.LoadFileName)) {
                OptionsParser.Usage($"{_Options.LoadFileName} does not exist");
            }

            _Host.StartVirtualRadarServer();
            try {
                var feedConfig = _Options.ParseMessage
                    ? _FeedFactory.GetConfig(_Options.FeedFormat)
                    : null;
                var chunker = feedConfig?.CreateChunker();
                IStreamChunkerState chunkerState = null;

                var countParcels = 0L;

                await WriteLine($"Opening {_Options.LoadFileName}");
                using (var stream = new FileStream(_Options.LoadFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    await WriteLine($"Initialising feed reader with stream");
                    await _Reader.InitialiseStreamAsync(stream, leaveOpen: true);
                    await WriteLine();

                    Parcel parcel;
                    do {
                        parcel = await _Reader.GetNextAsync(CancellationToken.None);
                        if(parcel != null) {
                            if(countParcels++ == 0) {
                                await DumpHeader(_Reader.Header);
                            }
                            await DumpParcel(_Reader.Header, parcel, countParcels);

                            if(chunker != null) {
                                chunkerState = DumpMessages(chunker, parcel.Packet, chunkerState);
                            }

                            await WriteLine();
                        }
                    } while(parcel != null);
                }
            } finally {
                _Host.StopVirtualRadarServer();
            }

            return true;
        }

        private async Task DumpHeader(Header header)
        {
            await WriteLine($"HEADER");
            await WriteLine($"------");
            await WriteLine($"Version: {header.Version} ({(header.IsVersionValid ? "valid" : "invalid")})");
            await WriteLine($"Started: {header.RecordingStartedUtc} {(header.RecordingStartedUtc.Kind == DateTimeKind.Utc ? "UTC" : "NOT UTC!")}");
            await WriteLine();

            if(!_Options.Show) {
                await WriteLine($"PARCELS");
                await WriteLine($"-------");
            }
        }

        private async Task DumpParcel(Header header, Parcel parcel, long parcelNumber)
        {
            var time = header.RecordingStartedUtc.AddMilliseconds(parcel.MillisecondReceived);

            if(!_Options.Show) {
                await WriteLine($"Parcel {parcelNumber,6} packet length {parcel.Packet.Length,6} at offset {parcel.MillisecondReceived} ms {time} UTC");
            } else {
                var heading = $"PARCEL {parcelNumber}";
                await WriteLine(heading);
                await WriteLine(new string('-', heading.Length));
                await WriteLine($"Received:       Offset {parcel.MillisecondReceived} ms ({time} UTC)");
                await WriteLine($"Packet length:  {parcel.Packet.Length} bytes");
                foreach(var line in _HexDump.DumpBuffer(parcel.Packet)) {
                    if(line.StartsWith("        ")) {
                        await WriteLine($"Packet:{line[1..]}");
                    } else {
                        await WriteLine($"      {line}");
                    }
                }
            }
        }

        private IStreamChunkerState DumpMessages(StreamChunker chunker, byte[] packet, IStreamChunkerState chunkerState)
        {
            void chunkRead(object _, ReadOnlyMemory<byte> chunk)
            {
                Console.WriteLine();
                Console.WriteLine($"Message {chunker.CountChunksExtracted}");
                foreach (var line in _HexDump.DumpBuffer(chunk)) {
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
