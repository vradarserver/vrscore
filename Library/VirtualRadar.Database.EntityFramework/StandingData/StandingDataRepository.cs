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
using Microsoft.EntityFrameworkCore;
using VirtualRadar.StandingData;

namespace VirtualRadar.Database.EntityFramework.StandingData
{
    class StandingDataRepository(
        IFileSystem _FileSystem,
        IWorkingFolder _WorkingFolder
    ) : IStandingDataRepository
    {
        private readonly object _EFSingleThreadLock = new();
        private Entities.CodeBlock[] _CodeBlockCache;

        /// <inheritdoc/>
        public AircraftType AircraftType_GetByCode(string code)
        {
            AircraftType result = null;

            if(!String.IsNullOrEmpty(code)) {
                lock(_EFSingleThreadLock) {
                    try {
                        using(var context = CreateContext()) {
                            var records = context
                                .AircraftTypeNoEnumsViews
                                .Where(view => view.Icao == code)
                                .AsNoTracking();
                            result = Entities.AircraftTypeNoEnumsView.ToAircraftType(records);
                        }
                    } catch(FileNotFoundException) {
                        ;
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Airline> Airlines_GetByCode(string code)
        {
            Airline[] result = null;

            if(!String.IsNullOrWhiteSpace(code) && code.Length >= 2 && code.Length <= 3) {
                lock(_EFSingleThreadLock) {
                    try {
                        using(var context = CreateContext()) {
                            IQueryable<Entities.Operator> set = context.Operators;

                            if(code.Length == 2) {
                                set = set.Where(op => op.Iata == code);
                            } else {
                                set = set.Where(op => op.Icao == code);
                            }

                            result = set
                                .AsNoTracking()
                                .Select(op => op.ToAirline())
                                .ToArray();
                        }
                    } catch(FileNotFoundException) {
                        ;
                    }
                }
            }

            return result ?? [];
        }

        /// <inheritdoc/>
        public Airport Airport_GetByCode(string code)
        {
            Airport result = null;

            if(!String.IsNullOrEmpty(code) && code.Length >= 3 && code.Length <= 4) {
                lock(_EFSingleThreadLock) {
                    try {
                        using(var context = CreateContext()) {
                            IQueryable<Entities.Airport> set = context.Airports;

                            if(code.Length == 3) {
                                set = set.Where(airport => airport.Iata == code);
                            } else {
                                set = set.Where(airport => airport.Icao == code);
                            }

                            result = set
                                .Include(airport => airport.Country)
                                .AsNoTracking()
                                .Select(airport => airport.ToAirport())
                                .FirstOrDefault();
                        }
                    } catch(FileNotFoundException) {
                        ;
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public CodeBlock CodeBlock_GetForIcao24(Icao24 icao24)
        {
            CodeBlock result = null;

            if(icao24.IsValid) {
                var codeBlocks = GetCodeBlocks();
                foreach(var codeBlock in codeBlocks) {
                    if((codeBlock.SignificantBitMask & icao24) == codeBlock.BitMask) {
                        result = codeBlock.ToCodeBlock();
                        break;
                    }
                }
                result ??= new();
            }

            return result;
        }

        private Entities.CodeBlock[] GetCodeBlocks()
        {
            var result = _CodeBlockCache;
            if(result == null) {
                lock(_EFSingleThreadLock) {
                    try {
                        using(var context = CreateContext()) {
                            result = context
                                .CodeBlocks
                                .Include(codeBlock => codeBlock.Country)
                                .AsNoTracking()
                                .OrderByDescending(codeBlock => codeBlock.SignificantBitMask)
                                .ThenBy(codeBlock => codeBlock.IsMilitary ? 0 : 1)
                                .ToArray();
                            _CodeBlockCache = result;
                        }
                    } catch(FileNotFoundException) {
                        ;
                    }
                }
            }

            return result ?? [];
        }

        /// <inheritdoc/>
        public Route Route_GetForCallsign(string callsign)
        {
            Route result = null;

            if(!String.IsNullOrWhiteSpace(callsign)) {
                lock(_EFSingleThreadLock) {
                    try {
                        using(var context = CreateContext()) {
                            result = context
                                .Routes
                                .AsNoTracking()
                                //.AsSplitQuery()
                                .Where(route => route.Callsign == callsign)
                                .Include(route => route.FromAirport)
                                .ThenInclude(airport => airport.Country)
                                .Include(route => route.ToAirport)
                                .ThenInclude(airport => airport.Country)
                                .Include(route => route.RouteStops)
                                .ThenInclude(stop => stop.Airport)
                                .ThenInclude(airport => airport.Country)
                                .Select(route => route.ToRoute())
                                .FirstOrDefault();
                        }
                    } catch(FileNotFoundException) {
                        ;
                    }
                }
            }

            return result;
        }

        private StandingDataContext CreateContext()
        {
            return new StandingDataContext(_FileSystem, _WorkingFolder);
        }
    }
}
