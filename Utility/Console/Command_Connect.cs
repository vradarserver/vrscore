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
using System.Net;
using VirtualRadar.CommandLine;
using VirtualRadar.Connection;

namespace VirtualRadar.Utility.CLIConsole
{
    public class Command_Connect(
        #pragma warning disable IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
        ReceiveConnectorFactory _ConnectorFactory,
        HeaderService _Header
        #pragma warning restore IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
    ) : CommonCommand
    {
        public IPAddress Address { get; set; } = IPAddress.None;

        public int Port { get; set; }

        public bool ShowContent { get; set; }

        public FileInfo? SaveFileInfo { get; set; }

        public async Task<bool> RunAsync()
        {
            await _Header.OutputCopyrightAsync();
            await _Header.OutputTitleAsync("Connect TCP");
            await _Header.OutputOptionsAsync(
                ("Address",         Address.ToString()),
                ("Port",            Port.ToString()),
                ("Show Content",    ShowContent.ToString()),
                ("Save FileName",   SaveFileInfo?.ToString())
            );

            var hexDump = ShowContent
                ? new HexDump() { EmitHeader = false, }
                : null;

            await using(var fileStream = SaveFileInfo?.OpenWrite()) {
                await using(var connector = CreateConnector(hexDump, fileStream)) {
                    await StreamFromConnectorAsync(connector, hexDump, fileStream);
                }
            }

            return true;
        }

        private IConnector CreateConnector(HexDump? hexDump, FileStream? fileStream)
        {
            var connectorSettingsDto = new TcpPullConnectorSettingsDto() {
                Address =   Address.ToString(),
                Port =      Port
            };
            var connector = _ConnectorFactory.Create(connectorSettingsDto)
                ?? throw new InvalidOperationException($"Could not create a connector for {connectorSettingsDto}");

            connector.ConnectionStateChanged += (_,_) => Console.WriteLine($"Connection is now {connector.ConnectionState}");

            if(hexDump != null || fileStream != null) {
                connector.PacketReceived += (_,packet) => {
                    if(hexDump != null) {
                        foreach(var line in hexDump.DumpBuffer(packet)) {
                            Console.WriteLine(line);
                        }
                    }
                    if(fileStream != null) {
                        fileStream.Write(packet.Span);
                    }
                };
            }

            return connector;
        }

        private async Task StreamFromConnectorAsync(IConnector connector, HexDump? hexDump, FileStream? fileStream)
        {
            using(var cancelSource = new CancellationTokenSource()) {
                await Console.Out.WriteLineAsync($"Opening stream (connector is currently {connector.ConnectionState})");
                await connector.OpenAsync(cancelSource.Token);

                if(hexDump != null || fileStream != null) {
                    if(fileStream != null) {
                        await WriteLineAsync($"Copying TCP stream to {SaveFileInfo}");
                    }
                    await WriteLineAsync("Press any key to stop");
                    await CancelOnKeyPress.IfAnyKeyPressed(cancelSource);
                }

                await WriteLineAsync($"Cleaning up stream");
                await connector.CloseAsync();
            }
        }
    }
}
