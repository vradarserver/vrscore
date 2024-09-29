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
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using VirtualRadar.Connection;

namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_ConnectListener(
        IServiceProvider _ServiceProvider,
        Options _Options,
        HeaderService _Header
    ) : CommandRunner
    {
        public override async Task<bool> Run()
        {
            await _Header.OutputCopyright();
            await _Header.OutputTitle("Connect TCP");
            await _Header.OutputOptions(
                ("Address",         _Options.Address),
                ("Port",            _Options.Port.ToString()),
                ("Show Content",    _Options.Show.ToString()),
                ("Save FileName",   _Options.SaveFileName)
            );

            if(!IPAddress.TryParse(_Options.Address, out var ipAddress)) {
                OptionsParser.Usage($"Cannot parse \"{_Options.Address}\" into an IP address");
            }

            var cancelSource = new CancellationTokenSource();

            await WriteLine($"Creating TCP pull connector to {ipAddress}:{_Options.Port}");
            var connectorFactory = _ServiceProvider.GetRequiredService<ReceiveConnectorFactory>();
            var connectorOptions = new TcpPullConnectorSettings() {
                Address =   ipAddress.ToString(),
                Port =      _Options.Port
            };
            var connector = connectorFactory.Create(connectorOptions);

            var hexDump = _Options.Show
                ? new HexDump() { EmitHeader = false, }
                : null;

            FileStream fileStream = null;
            if(!String.IsNullOrEmpty(_Options.SaveFileName)) {
                var folder = Path.GetDirectoryName(_Options.SaveFileName);
                if(folder != "" && !Directory.Exists(folder)) {
                    await Console.Out.WriteLineAsync("Creating directory {folder}");
                    Directory.CreateDirectory(folder);
                }

                fileStream = new FileStream(_Options.SaveFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            }

            connector.ConnectionStateChanged += (_,_) => Console.WriteLine($"Connection is now {connector.ConnectionState}");

            connector.PacketReceived += (_,packet) => {
                if(hexDump != null) {
                    foreach(var line in hexDump.DumpBuffer(packet)) {
                        Console.Out.WriteLineAsync(line);
                    }
                }
                if(fileStream != null) {
                    fileStream.Write(packet.Span);
                }
            };

            try {
                await WriteLine($"Opening stream (connector is currently {connector.ConnectionState})");
                await connector.OpenAsync(cancelSource.Token);

                if(hexDump != null || fileStream != null) {
                    await WriteLine($"Copying TCP stream to {_Options.SaveFileName}");
                    await WriteLine("Press any key to stop");
                    await CancelIfAnyKeyPressed(cancelSource);
                }

                await WriteLine($"Cleaning up stream");
                await connector.CloseAsync();
            } finally {
                await connector.DisposeAsync();
                if(fileStream != null) {
                    fileStream.Flush();
                    fileStream.Dispose();
                    fileStream = null;
                }
            }

            return true;
        }
    }
}
