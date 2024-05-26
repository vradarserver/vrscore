// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_Lookup : CommandRunner
    {
        private Options _Options;
        private HeaderService _Header;
        private IAircraftOnlineLookupProvider _AircraftLookupProvider;

        public CommandRunner_Lookup(Options options, HeaderService header, IAircraftOnlineLookupProvider aircraftLookupProvider)
        {
            _Options = options;
            _Header = header;
            _AircraftLookupProvider = aircraftLookupProvider;
        }

        public override async Task<bool> Run()
        {
            await _Header.OutputCopyright();
            await _Header.OutputTitle("Lookup");
            await _Header.OutputOptions(
                ("Entity",  _Options.LookupEntity.ToString()),
                ("Id",      _Options.Id)
            );
            await WriteLine();

            switch (_Options.LookupEntity) {
                case LookupEntity.Aircraft:     await LookupAircraft(); break;
                default:                        throw new NotImplementedException();
            }

            return true;
        }

        private async Task LookupAircraft()
        {
            var icaos = _Options.Id.Split([ "-", ], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var icao24s = new List<Icao24>();
            foreach(var icao in icaos) {
                if(!Icao24.TryParse(icao, out var icao24)) {
                    OptionsParser.Usage($"{icao} cannot be parsed into an ICAO24");
                }
                icao24s.Add(icao24);
            }
            if(icao24s.Count == 0) {
                OptionsParser.Usage("Missing IDs");
            }

            await WriteLine($"{Timestamp} Initialising supplier details");
            await _AircraftLookupProvider.InitialiseSupplierDetails(CancellationToken.None);

            await WriteLine($"{Timestamp} Supplier details:");
            await _Header.OutputOptions(
                ($"{Timindent} {nameof(_AircraftLookupProvider.DataSupplier)}",                  _AircraftLookupProvider.DataSupplier),
                ($"{Timindent} {nameof(_AircraftLookupProvider.MaxBatchSize)}",                  _AircraftLookupProvider.MaxBatchSize.ToString()),
                ($"{Timindent} {nameof(_AircraftLookupProvider.MaxSecondsAfterFailedRequest)}",  _AircraftLookupProvider.MaxSecondsAfterFailedRequest.ToString()),
                ($"{Timindent} {nameof(_AircraftLookupProvider.MinSecondsBetweenRequests)}",     _AircraftLookupProvider.MinSecondsBetweenRequests.ToString()),
                ($"{Timindent} {nameof(_AircraftLookupProvider.SupplierCredits)}",               _AircraftLookupProvider.SupplierCredits),
                ($"{Timindent} {nameof(_AircraftLookupProvider.SupplierWebSiteUrl)}",            _AircraftLookupProvider.SupplierWebSiteUrl)
            );

            await WriteLine($"{Timestamp} Looking up details for {String.Join(", ", icao24s.Select(r => r.ToString()))}");

            var outcomes = await _AircraftLookupProvider.LookupIcaos(icao24s, CancellationToken.None);
            await WriteLine($"{Timestamp} Found {outcomes.Found.Count:N0}, {outcomes.Missing.Count:N0} missing");
            await WriteLine();
            await WriteLine("FOUND");
            await WriteLine("-----");
            for(var idx = 0;idx < outcomes.Found.Count;++idx) {
                var found = outcomes.Found[idx];
                if(idx != 0) {
                    await WriteLine();
                }
                await WriteLine($"{found.Icao24.ToString()} Reg [{found.Registration}] Model [{found.ModelIcao}] [{found.Manufacturer}] [{found.Model}]");
                await WriteLine($"       Operator [{found.OperatorIcao}] [{found.Operator}]");
                await WriteLine($"       Country [{found.Country}] Serial [{found.Serial}] Year built [{found.YearFirstFlight}]");
            }
            await WriteLine();
            await WriteLine("MISSING");
            await WriteLine("-------");
            await WriteLine(String.Join(", ", outcomes.Missing.Select(r => r.Icao24.ToString())));
        }
    }
}
