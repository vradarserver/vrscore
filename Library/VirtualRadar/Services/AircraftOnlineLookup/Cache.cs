// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using LiteDB;
using Microsoft.Extensions.Options;
using VirtualRadar.Configuration;
using VirtualRadar.Message;

namespace VirtualRadar.Services.AircraftOnlineLookup
{
    class Cache : IAircraftOnlineLookupCache
    {
        private readonly object _SyncLock = new();
        private readonly IFileSystem _FileSystem;
        private readonly WorkingFolder _WorkingFolder;
        private readonly IOptions<AircraftOnlineLookupCacheConfig> _Options;

        public string FileName => _FileSystem.Combine(
            _WorkingFolder.Folder,
            "AircraftOnlineLookupCache.litedb"
        );

        /// <inheritdoc/>
        public bool Enabled => true;

        /// <inheritdoc/>
        public int ReadPriority => int.MaxValue;

        /// <inheritdoc/>
        public int WritePriority => int.MaxValue;

        /// <inheritdoc/>
        public bool CanRead => true;

        /// <inheritdoc/>
        public bool CanWrite => true;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="workingFolder"></param>
        /// <param name="fileSystem"></param>
        public Cache(
            WorkingFolder workingFolder,
            IFileSystem fileSystem,
            IOptions<AircraftOnlineLookupCacheConfig> options
        )
        {
            _WorkingFolder = workingFolder;
            _FileSystem = fileSystem;
            _Options = options;
        }

        /// <inheritdoc/>
        public Task<BatchedLookupOutcome> Read(IEnumerable<Icao24> icaos)
        {
            var result = new BatchedLookupOutcome();

            var hitThreshold = DateTime.UtcNow.AddDays(-_Options.Value.HitLifetimeDays);
            var missThreshold = DateTime.UtcNow.AddHours(-_Options.Value.MissLifetimeHours);

            using(var database = GetDatabase()) {
                var collection = GetCacheRecordCollection(database);
                collection.EnsureIndex(r => r.Icao24, unique: true);

                foreach(int icao24 in icaos) {
                    var cachedRecord = collection.Query()
                        .Where(cached => cached.Icao24 == icao24)
                        .FirstOrDefault();
                    if(cachedRecord != null) {
                        z`if(cachedRecord.Success && cachedRecord.UpdatedUtc >= hitThreshold) {
                            result.Found.Add(cachedRecord.ToLookupOutcome());
                        } else if(!cachedRecord.Success && cachedRecord.UpdatedUtc >= missThreshold) {
                            result.Missing.Add(cachedRecord.ToLookupOutcome());
                        }
                    }
                }
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task Write(BatchedLookupOutcome outcome, bool hasBeenSavedByAnotherCache)
        {
            if(!hasBeenSavedByAnotherCache) {
                using(var database = GetDatabase()) {
                    var collection = GetCacheRecordCollection(database);
                    foreach(var lookup in outcome.AllOutcomes) {
                        collection.Upsert(new CacheRecord(lookup));
                    }
                }
            }

            return Task.CompletedTask;
        }

        private LiteDatabase GetDatabase()
        {
            return new LiteDatabase($"Filename={FileName}; Connection=shared");
        }

        private ILiteCollection<CacheRecord> GetCacheRecordCollection(LiteDatabase database)
        {
            lock(_SyncLock) {
                if(database == null) {
                    database = new(FileName);
                    database.Pragma("USER_VERSION", 1);
                    database.Pragma("UTC_DATE",     true);
                }
            }

            return database.GetCollection<CacheRecord>("aclookup");
        }
    }
}
