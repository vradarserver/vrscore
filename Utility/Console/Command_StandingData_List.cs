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
using VirtualRadar.Extensions;
using VirtualRadar.StandingData;
using WindowProcessor;

namespace VirtualRadar.Utility.CLIConsole
{
    public class Command_StandingData_List(
        #pragma warning disable IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
        HeaderService                       _Header,
        IStandingDataRepository             _StandingDataRepository,
        IStandingDataOverridesRepository    _StandingDataOverridesRepository
        #pragma warning restore IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
    ) : CommonCommand
    {
        public StandingDataEntity Entity { get; set; }

        public string Code { get; set; } = "";

        public async Task<bool> RunAsync()
        {
            await _Header.OutputCopyrightAsync();
            await _Header.OutputTitleAsync("Standing Data");
            await _Header.OutputOptionsAsync(
                ("Entity", Entity.ToString()),
                ("Code",   Code)
            );
            await WriteLineAsync();

            if(String.IsNullOrWhiteSpace(Code)) {
                throw new BadParameterException(Commands.StandingDataListCommand, "Missing code");
            }

            switch(Entity) {
                case StandingDataEntity.AircraftType:
                    await DumpAircraftTypeAsync(_StandingDataRepository
                        .AircraftType_GetByCode(Code)
                    );
                    break;
                case StandingDataEntity.Airline:
                    await DumpAirlinesAsync(_StandingDataRepository
                        .Airlines_GetByCode(Code)
                        .OrderBy(r => r.Name)
                    );
                    break;
                case StandingDataEntity.Airport:
                    await DumpAirportAsync(_StandingDataRepository
                        .Airport_GetByCode(Code)
                    );
                    break;
                case StandingDataEntity.CodeBlock:
                    if(!Icao24.TryParse(Code, out var icao24)) {
                        throw new BadParameterException(Commands.StandingDataListCommand, $"{Code} is not a valid ICAO24");
                    }

                    _StandingDataOverridesRepository.Load();

                    await DumpCodeBlockAndOverrideAsync(
                        _StandingDataRepository.CodeBlock_GetForIcao24(icao24),
                        _StandingDataOverridesRepository.CodeBlockOverrideFor(icao24)
                    );
                    break;
                case StandingDataEntity.Route:
                    await DumpRouteAsync(_StandingDataRepository
                        .Route_GetForCallsign(Code)
                    );
                    break;
                default:
                    throw new NotImplementedException();
            }

            return true;
        }

        private async Task DumpAircraftTypeAsync(AircraftType? aircraftType)
        {
            if(aircraftType == null) {
                await WriteLineAsync("None");
            } else {
                await WriteLineAsync(
                    $"Type [{aircraftType.Type}] Species [{aircraftType.Species}] WTC [{aircraftType.WakeTurbulenceCategory}] " +
                    $"Engines [{aircraftType.Engines} × {aircraftType.EngineType}] Placement [{aircraftType.EnginePlacement}]"
                );
                await WriteLineAsync();
                await WriteLineAsync($"{"Manufacturer",-40} {"Model",-40}");
                await WriteLineAsync($"{new String('-',40)} {new String('-',40)}");
                for(var idx = 0;idx < aircraftType.Manufacturers.Count;++idx) {
                    var manufacturer = aircraftType.Manufacturers[idx];
                    var model = idx < aircraftType.Models.Count ? aircraftType.Models[idx] : "";
                    await WriteLineAsync($"{manufacturer.TruncateAt(40),-40} {model.TruncateAt(40),-40}");
                }
            }
        }

        private async Task DumpAirlinesAsync(IEnumerable<Airline> airlines)
        {
            var table = new ConsoleTable<Airline>([
                (new("ICAO", 4),        row => row.IcaoCode),
                (new("IATA", 4),        row => row.IataCode),
                (new("Name", 50),       row => row.Name),
                (new("Charter", 20),    row => row.CharterFlightPattern),
                (new("Posn", 20),       row => row.PositioningFlightPattern)
            ]);
            await table.Dump(airlines);
        }

        private async Task DumpAirportAsync(Airport? airport)
        {
            if(airport == null) {
                await WriteLineAsync("None");
            } else {
                await DumpAirportsAsync([ airport ]);
            }
        }

        private async Task DumpAirportsAsync(IEnumerable<Airport> airports)
        {
            var table = new ConsoleTable<Airport>([
                (new("ICAO", 4),                        row => row.IcaoCode),
                (new("IATA", 4),                        row => row.IataCode),
                (new("Name", 50),                       row => row.Name),
                (new("Country", 30),                    row => row.Country),
                (new("Latitude", 11, Alignment.Right),  row => row.Location?.Latitude.ToString("N6")),
                (new("Longitude", 11, Alignment.Right), row => row.Location?.Longitude.ToString("N6")),
                (new("Altitude", 8, Alignment.Right),   row => row.AltitudeFeet?.ToString("N0")),
            ]);
            await table.Dump(airports);
        }

        private async Task DumpCodeBlockAndOverrideAsync(CodeBlock? codeBlock, CodeBlock? overrideCodeBlock)
        {
            var table = new ConsoleTable<CodeBlock>([
                (new("Country", 30),                    row => row.Country),
                (new("Military", 8, Alignment.Centre),  row => row.IsMilitary ? "Yes" : "No"),
            ]);

            if(codeBlock == null) {
                await WriteLineAsync("None");
            } else {
                await table.Dump([ codeBlock ]);
            }

            if(overrideCodeBlock != null) {
                await WriteLineAsync();
                await WriteLineAsync("Overridden by local code block:");
                await WriteLineAsync();
                await table.Dump([ overrideCodeBlock ]);
            }
        }

        private async Task DumpRouteAsync(Route? route)
        {
            if(route == null) {
                await WriteLineAsync("None");
            } else {
                await DumpAirportsAsync(new Airport[] { route.From }
                    .Concat(route.Stopovers)
                    .Concat([ route.To ])
                );
            }
        }
    }
}
