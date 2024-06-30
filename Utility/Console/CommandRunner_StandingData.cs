// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Extensions;
using VirtualRadar.StandingData;
using WindowProcessor;

namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_StandingData(
        Options                 _Options,
        HeaderService           _Header,
        IStandingDataUpdater    _StandingDataUpdater,
        IWorkingFolder          _WorkingFolder,
        IStandingDataRepository _StandingDataRepository
    ) : CommandRunner
    {
        public async override Task<bool> Run()
        {
            if((_Options.List && _Options.Update) || (!_Options.List && !_Options.Update)) {
                OptionsParser.Usage("Specify one of -list or -update");
            }
            await _Header.OutputCopyright();
            await _Header.OutputTitle("Standing Data");

            if(_Options.List) {
                await List();
            } else {
                await Update();
            }

            return true;
        }

        private async Task List()
        {
            await _Header.OutputOptions(
                ("Entity", _Options.StandingDataEntity.ToString()),
                ("Code",   _Options.Code)
            );
            await WriteLine();

            if(String.IsNullOrWhiteSpace(_Options.Code)) {
                OptionsParser.Usage("Missing code");
            }

            switch(_Options.StandingDataEntity) {
                case StandingDataEntity.AircraftType:
                    await DumpAircraftType(_StandingDataRepository
                        .AircraftType_GetByCode(_Options.Code)
                    );
                    break;
                case StandingDataEntity.Airline:
                    await DumpAirlines(_StandingDataRepository
                        .Airlines_GetByCode(_Options.Code)
                        .OrderBy(r => r.Name)
                    );
                    break;
                case StandingDataEntity.Airport:
                    await DumpAirports([ _StandingDataRepository
                        .Airport_GetByCode(_Options.Code)
                    ]);
                    break;
                case StandingDataEntity.CodeBlock:
                    if(!Icao24.TryParse(_Options.Code, out var icao24)) {
                        OptionsParser.Usage($"{_Options.Code} is not a valid ICAO24");
                    }
                    await DumpCodeBlock(_StandingDataRepository
                        .CodeBlock_GetForIcao24(icao24)
                    );
                    break;
                case StandingDataEntity.Route:
                    await DumpRoute(_StandingDataRepository
                        .Route_GetForCallsign(_Options.Code)
                    );
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task DumpAircraftType(AircraftType aircraftType)
        {
            if(aircraftType == null) {
                await WriteLine("None");
            } else {
                await WriteLine(
                    $"Type [{aircraftType.Type}] Species [{aircraftType.Species}] WTC [{aircraftType.WakeTurbulenceCategory}] " +
                    $"Engines [{aircraftType.Engines} × {aircraftType.EngineType}] Placement [{aircraftType.EnginePlacement}]"
                );
                await WriteLine();
                await WriteLine($"{"Manufacturer",-40} {"Model",-40}");
                await WriteLine($"{new String('-',40)} {new String('-',40)}");
                for(var idx = 0;idx < aircraftType.Manufacturers.Count;++idx) {
                    var manufacturer = aircraftType.Manufacturers[idx];
                    var model = idx < aircraftType.Models.Count ? aircraftType.Models[idx] : "";
                    await WriteLine($"{manufacturer.TruncateAt(40),-40} {model.TruncateAt(40),-40}");
                }
            }
        }

        private async Task DumpAirlines(IEnumerable<Airline> airlines)
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

        private async Task DumpAirports(IEnumerable<Airport> airports)
        {
            var table = new ConsoleTable<Airport>([
                (new("ICAO", 4),                        row => row.IcaoCode),
                (new("IATA", 4),                        row => row.IataCode),
                (new("Name", 50),                       row => row.Name),
                (new("Country", 30),                    row => row.Country),
                (new("Latitude", 11, Alignment.Right),  row => row.Latitude?.ToString("N6")),
                (new("Longitude", 11, Alignment.Right), row => row.Longitude?.ToString("N6")),
                (new("Altitude", 8, Alignment.Right),   row => row.AltitudeFeet?.ToString("N0")),
            ]);
            await table.Dump(airports);
        }

        private async Task DumpCodeBlock(CodeBlock codeBlock)
        {
            if(codeBlock == null) {
                await WriteLine("None");
            } else {
                var table = new ConsoleTable<CodeBlock>([
                    (new("Country", 30),                    row => row.Country),
                    (new("Military", 8, Alignment.Centre),  row => row.IsMilitary ? "Yes" : "No"),
                ]);
                await table.Dump([ codeBlock ]);
            }
        }

        private async Task DumpRoute(Route route)
        {
            if(route == null) {
                await WriteLine("None");
            } else {
                await DumpAirports(new Airport[] { route.From }
                    .Concat(route.Stopovers)
                    .Concat(new Airport[] { route.To })
                );
            }
        }

        private async Task Update()
        {
            await WriteLine();
            if(await _StandingDataUpdater.DataIsOld(CancellationToken.None) == false) {
                await WriteLine($"Already up-to-date, nothing downloaded");
            } else {
                await WriteLine($"SDM file is out of date or missing, downloading");
                await _StandingDataUpdater.Update(CancellationToken.None);
                await WriteLine($"Downloaded into {_WorkingFolder.Folder}");
            }
        }
    }
}
