// Copyright © 2026 onwards, Andrew Whewell
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
using VirtualRadar.CommandLine;
using VirtualRadar.Connection;
using VirtualRadar.Feed.Recording;

namespace VirtualRadar.Utility.CLIConsole
{
    public class Command_Record(
        #pragma warning disable IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
        ReceiveConnectorFactory _ConnectorFactory,
        HeaderService _Header,
        IRecorder _FeedRecorder
        #pragma warning restore IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
    ) : CommonCommand
    {
        public IPAddress Address { get; set; } = IPAddress.None;

        public int Port { get; set; }

        public FileInfo SaveFileInfo { get; set; } = null!;

        public async Task<bool> RunAsync()
        {
            await _Header.OutputCopyrightAsync();
            await _Header.OutputTitleAsync("Record Feed");
            await _Header.OutputOptionsAsync(
                ("Address",         Address.ToString()),
                ("Port",            Port.ToString()),
                ("Save FileName",   SaveFileInfo.ToString())
            );

            await WriteLineAsync($"Opening {SaveFileInfo} for writing");

            await using(var fileStream = SaveFileInfo.OpenWrite()) {
                await _FeedRecorder.WriteHeaderAsync(fileStream);

                await using(var connector = CreateConnector(fileStream)) {
                    await RecordFromConnectorAsync(connector);
                }
            }

            SaveFileInfo.Refresh();
            await WriteLineAsync($"Wrote {SaveFileInfo.Length:N0} bytes to {SaveFileInfo}");

            return true;
        }

        private IConnector CreateConnector(FileStream fileStream)
        {
            var connectorSettingsDto = new TcpPullConnectorSettingsDto() {
                Address =   Address.ToString(),
                Port =      Port,
            };
            var connector = _ConnectorFactory.Create(connectorSettingsDto)
                ?? throw new InvalidOperationException($"Could not create a connector for {connectorSettingsDto}");

            connector.ConnectionStateChanged += (_,_) => Console.WriteLine($"Connection is now {connector.ConnectionState}");

            var totalReadLength = 0L;
            var packetCount = 0L;
            var stopwatch = Stopwatch.StartNew();

            connector.PacketReceived += (_,packet) => {
                if(packetCount != 0) {
                    Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                }
                ++packetCount;
                totalReadLength += packet.Length;
                Console.WriteLine($"{stopwatch.Elapsed} read packet {packetCount} length {packet.Length:N0} for {totalReadLength:N0} total           ");
                _FeedRecorder.WritePacketAsync(fileStream, packet);
            };

            return connector;
        }

        private async Task RecordFromConnectorAsync(IConnector connector)
        {
            using(var cancelSource = new CancellationTokenSource()) {
                await WriteLineAsync($"Opening stream (connector is currently {connector.ConnectionState})");
                await connector.OpenAsync(cancelSource.Token);

                await WriteLineAsync($"Recording network feed to {SaveFileInfo}");
                await WriteLineAsync("Press any key to stop");
                await CancelOnKeyPress.IfAnyKeyPressed(cancelSource);

                await WriteLineAsync("Cleaning up stream");
                await connector.CloseAsync();
            }
        }
    }
}
