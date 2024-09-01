// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Extensions.Options;
using VirtualRadar.Configuration;
using VirtualRadar.Database.EntityFramework.AircraftOnlineLookupCache.Entities;
using VirtualRadar.Message;

namespace VirtualRadar.Database.EntityFramework.AircraftOnlineLookupCache
{
    class LookupCacheRepository(
        IFileSystem _FileSystem,
        IWorkingFolder _WorkingFolder,
        ISettings<AircraftOnlineLookupCacheSettings> _Settings
    ) : IAircraftOnlineLookupCache
    {
        private static bool _CreatedInThisSession;
        private static DatabaseVersion _DatabaseVersion;
        private readonly object _EFSingleThreadLock = new();

        /// <inheritdoc/>
        public bool Enabled { get; set; } = true;

        /// <inheritdoc/>
        public int ReadPriority => int.MinValue;

        /// <inheritdoc/>
        public bool CanRead => true;

        /// <inheritdoc/>
        public int WritePriority => int.MinValue;

        /// <inheritdoc/>
        public bool CanWrite => true;

        /// <inheritdoc/>
        public Task<BatchedLookupOutcome> Read(IEnumerable<Icao24> icaos)
        {
            var result = new BatchedLookupOutcome();

            if(icaos?.Any() ?? false) {
                lock(_EFSingleThreadLock) {
                    var settings = _Settings.LatestValue;
                    var utcNow = DateTime.UtcNow;
                    var hitThreshold = utcNow.AddDays(-settings.HitLifetimeDays);
                    var missThreshold = utcNow.AddHours(-settings.MissLifetimeHours);

                    using(var context = CreateContext()) {
                        foreach(var icao24 in icaos) {
                            var icao = icao24.ToString();
                            var aircraftDetail = context
                                .AircraftDetails
                                .Where(entity => entity.Icao == icao)
                                .FirstOrDefault();
                            if(aircraftDetail != null) {
                                if(!aircraftDetail.IsMissing && aircraftDetail.UpdatedUtc >= hitThreshold) {
                                    result.Found.Add(aircraftDetail.ToLookupOutcome());
                                } else if(aircraftDetail.IsMissing && aircraftDetail.UpdatedUtc >= missThreshold) {
                                    result.Missing.Add(aircraftDetail.ToLookupOutcome());
                                }
                            }
                        }
                    }
                }
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task Write(BatchedLookupOutcome outcome, bool hasBeenSavedByAnotherCache)
        {
            if(Enabled && !hasBeenSavedByAnotherCache) {
                lock(_EFSingleThreadLock) {
                    using(var context = CreateContext()) {
                        var utcNow = DateTime.UtcNow;
                        foreach(var lookup in outcome.AllOutcomes) {
                            var icao = lookup.Icao24.ToString();
                            var extant = context
                                .AircraftDetails
                                .Where(entity => entity.Icao == icao)
                                .FirstOrDefault();
                            if(extant == null) {
                                extant = new();
                                context.AircraftDetails.Add(extant);
                            }
                            extant.CopyFrom(lookup, utcNow);
                        }
                        context.SaveChanges();
                    }
                }
            }

            return Task.CompletedTask;
        }

        private LookupCacheContext CreateContext()
        {
            var context = new LookupCacheContext(
                _FileSystem,
                _WorkingFolder
            );
            if(!_CreatedInThisSession) {
                try {
                    context.EnsureCreated();

                    var databaseVersion = context.DatabaseVersions.FirstOrDefault();
                    if(databaseVersion == null) {
                        databaseVersion = new() {
                            Version = 1,
                        };
                        context.DatabaseVersions.Add(databaseVersion);
                        context.SaveChanges();
                    }

                    _DatabaseVersion = databaseVersion;
                    _CreatedInThisSession = true;
                } catch {
                    context.Dispose();
                    throw;
                }
            }

            return context;
        }
    }
}
