// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Net;
using VirtualRadar.Connection;

namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_ConnectTcpListener : CommandRunner
    {
        private Options _Options;
        private HeaderService _Header;
        private StreamDumperService _StreamDumper;

        public CommandRunner_ConnectTcpListener(Options options, HeaderService header, StreamDumperService streamDumper)
        {
            _Options = options;
            _Header = header;
            _StreamDumper = streamDumper;
        }

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

                if(_Options.Show || !String.IsNullOrEmpty(_Options.SaveFileName)) {
                    await WriteLine($"Copying TCP stream to {_Options.SaveFileName}");
                    await WriteLine("Press any key to stop");
                    var cts = new CancellationTokenSource();
                    var keyWatcherTask = CancelIfAnyKeyPressed(cts);
                    try {
                        await _StreamDumper.DumpStream(stream, _Options.Show, _Options.SaveFileName, cts.Token);
                    } catch(OperationCanceledException) {
                        await WriteLine();
                        await WriteLine("Dump cancelled");
                    } finally {
                        await keyWatcherTask;
                    }
                }

                await WriteLine($"Closing stream");
                await connector.CloseAsync();
                await WriteLine($"Connection state is now {connector.ConnectionState}");
            } finally {
                await connector.DisposeAsync();
            }

            return true;
        }
    }
}
