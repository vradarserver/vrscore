﻿// Copyright © 2024 onwards, Andrew Whewell
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
using VirtualRadar.Database.EntityFramework.StandingData.Entities;
using VirtualRadar.StandingData;

namespace VirtualRadar.Database.EntityFramework.StandingData
{
    class StandingDataRepository(
        IFileSystem _FileSystem,
        IWorkingFolder _WorkingFolder
    ) : IStandingDataRepository
    {
        private readonly object _EFSingleThreadLock = new();

        public IReadOnlyList<Airline> Airlines_GetByCode(string code)
        {
            Airline[] result = null;

            if(!String.IsNullOrWhiteSpace(code) && code.Length >= 2 && code.Length <= 3) {
                lock(_EFSingleThreadLock) {
                    try {
                        using(var context = CreateContext()) {
                            IQueryable<Operator> set = context.Operators;

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

        private StandingDataContext CreateContext()
        {
            return new StandingDataContext(_FileSystem, _WorkingFolder);
        }
    }
}
