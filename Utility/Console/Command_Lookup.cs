// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.CommandLine;

namespace VirtualRadar.Utility.CLIConsole
{
    public class Command_Lookup(
        #pragma warning disable IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
        HeaderService _Header,
        IAircraftOnlineLookupProvider _AircraftLookupProvider
        #pragma warning restore IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
    ) : CommonCommand
    {
        public LookupEntity Entity { get; set; }

        public Icao24[] Ids { get; set; } = [];

        public async Task<bool> RunAsync()
        {
            await _Header.OutputCopyrightAsync();
            await _Header.OutputTitleAsync("Lookup");
            await _Header.OutputOptionsAsync(
                ("Entity",  Entity.ToString()),
                ("Id",      String.Join(",", Ids.Select(r => r.ToString())))
            );
            await WriteLineAsync();

            switch(Entity) {
                case LookupEntity.Aircraft:     await LookupAircraftAsync(); break;
                default:                        throw new NotImplementedException();
            }

            return true;
        }

        private async Task LookupAircraftAsync()
        {
            await WriteLineAsync($"{Timestamp} Initialising supplier details");
            await _AircraftLookupProvider.InitialiseSupplierDetails(CancellationToken.None);

            await WriteLineAsync($"{Timestamp} Supplier details:");
            await _Header.OutputOptionsAsync(
                ($"{Timindent} {nameof(_AircraftLookupProvider.DataSupplier)}",                  _AircraftLookupProvider.DataSupplier),
                ($"{Timindent} {nameof(_AircraftLookupProvider.MaxBatchSize)}",                  _AircraftLookupProvider.MaxBatchSize.ToString()),
                ($"{Timindent} {nameof(_AircraftLookupProvider.MaxSecondsAfterFailedRequest)}",  _AircraftLookupProvider.MaxSecondsAfterFailedRequest.ToString()),
                ($"{Timindent} {nameof(_AircraftLookupProvider.MinSecondsBetweenRequests)}",     _AircraftLookupProvider.MinSecondsBetweenRequests.ToString()),
                ($"{Timindent} {nameof(_AircraftLookupProvider.SupplierCredits)}",               _AircraftLookupProvider.SupplierCredits),
                ($"{Timindent} {nameof(_AircraftLookupProvider.SupplierWebSiteUrl)}",            _AircraftLookupProvider.SupplierWebSiteUrl)
            );

            await WriteLineAsync($"{Timestamp} Looking up details for {String.Join(", ", Ids.Select(r => r.ToString()))}");

            var outcomes = await _AircraftLookupProvider.LookupIcaos(Ids, CancellationToken.None);
            await WriteLineAsync($"{Timestamp} Found {outcomes.Found.Count:N0}, {outcomes.Missing.Count:N0} missing");
            await WriteLineAsync();
            await WriteLineAsync("FOUND");
            await WriteLineAsync("-----");
            for(var idx = 0;idx < outcomes.Found.Count;++idx) {
                var found = outcomes.Found[idx];
                if(idx != 0) {
                    await WriteLineAsync();
                }
                await WriteLineAsync($"{found.Icao24.ToString()} Reg [{found.Registration}] Model [{found.ModelIcao}] [{found.Manufacturer}] [{found.Model}]");
                await WriteLineAsync($"       Operator [{found.OperatorIcao}] [{found.Operator}]");
                await WriteLineAsync($"       Country [{found.Country}] Serial [{found.Serial}] Year built [{found.YearBuilt}]");
            }
            await WriteLineAsync();
            await WriteLineAsync("MISSING");
            await WriteLineAsync("-------");
            await WriteLineAsync(String.Join(", ", outcomes.Missing.Select(r => r.Icao24.ToString())));
        }
    }
}
