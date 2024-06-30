// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
                default:
                    throw new NotImplementedException();
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
