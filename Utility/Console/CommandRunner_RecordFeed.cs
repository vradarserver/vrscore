// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Diagnostics;
using System.IO;
using System.Net;
using VirtualRadar.Connection;
using VirtualRadar.Feed.Recording;

namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_RecordFeed(Options _Options, HeaderService _Header, IRecorder _FeedRecorder) : CommandRunner
    {
        public override async Task<bool> Run()
        {
            await _Header.OutputCopyright();
            await _Header.OutputTitle("Record Feed");
            await _Header.OutputOptions(
                ("Address",         _Options.Address),
                ("Port",            _Options.Port.ToString()),
                ("Save FileName",   _Options.SaveFileName)
            );

            if(!IPAddress.TryParse(_Options.Address, out var ipAddress)) {
                OptionsParser.Usage($"Cannot parse \"{_Options.Address}\" into an IP address");
            }
            if(String.IsNullOrEmpty(_Options.SaveFileName)) {
                OptionsParser.Usage($"Missing filename");
            }

            var cancellationTokenSource = new CancellationTokenSource();

            await WriteLine($"Creating TCP connector to {ipAddress}:{_Options.Port}");
            var connector = new TcpConnector(new() {
                Address = ipAddress,
                Port = _Options.Port,
                CanRead = true,
                CanWrite = false
            });

            try {
                await WriteLine($"Opening stream (connector is currently {connector.ConnectionState})");
                var stream = await connector.OpenAsync(CancellationToken.None);
                await WriteLine($"Connection state is now {connector.ConnectionState}");

                await WriteLine($"Opening {_Options.SaveFileName} for writing");
                using(var fileStream = new FileStream(_Options.SaveFileName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    await _FeedRecorder.WriteHeaderAsync(fileStream);

                    await WriteLine($"Recording network feed to {_Options.SaveFileName}");
                    await WriteLine($"Press any key to stop");
                    var keyWatcherTask = CancelIfAnyKeyPressed(cancellationTokenSource);

                    try {
                        var readBuffer = new byte[64 * 1024];
                        var readLength = 0;
                        var totalReadLength = 0L;
                        var packetCount = 0L;
                        var stopwatch = Stopwatch.StartNew();

                        do {
                            readLength = await stream.ReadAsync(readBuffer, cancellationTokenSource.Token);
                            if(!cancellationTokenSource.IsCancellationRequested) {
                                if(packetCount != 0) {
                                    Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                                }
                                ++packetCount;
                                totalReadLength += readLength;
                                await WriteLine($"{stopwatch.ToString()} read packet {packetCount} length {readLength:N0} for {totalReadLength:N0} total           ");
                                await _FeedRecorder.WritePacketAsync(fileStream, readBuffer.AsMemory(0, readLength));
                            }
                        } while(readLength != 0);

                        cancellationTokenSource.Cancel();
                    } catch(OperationCanceledException) {
                        await WriteLine();
                        await WriteLine("Recording stopped");
                    } finally {
                        cancellationTokenSource.Cancel();
                        await keyWatcherTask;
                    }

                    await fileStream.FlushAsync();
                }

                await Console.Out.WriteLineAsync($"Wrote {new FileInfo(_Options.SaveFileName).Length:N0} bytes to {_Options.SaveFileName}");
                await connector.CloseAsync();
            } finally {
                await connector.DisposeAsync();
            }

            return true;
        }
    }
}
