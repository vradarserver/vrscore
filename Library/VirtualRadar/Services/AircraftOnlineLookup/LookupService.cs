// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Message;

namespace VirtualRadar.Services.AircraftOnlineLookup
{
    /// <inheritdoc/>
    class LookupService : IAircraftOnlineLookupService
    {
        private readonly object _SyncLock = new();
        private readonly IAircraftOnlineLookupProvider _Provider;
        private readonly Dictionary<Icao24, DateTime> _LookupMap = new();       // <-- value is the time at UTC when the lookup was added
        private readonly IAircraftOnlineLookupCache[] _Caches;
        private readonly System.Threading.Timer _Timer;

        private bool _ProviderInitialised;
        private int _TimerPauseSeconds = 1;

        /// <inheritdoc/>
        public event EventHandler<BatchedLookupOutcome> LookupCompleted;

        /// <summary>
        /// Raises <see cref="LookupCompleted"/>.
        /// </summary>
        /// <param name="batchedOutcome"></param>
        private void OnLookupCompleted(BatchedLookupOutcome batchedOutcome)
        {
            LookupCompleted?.Invoke(this, batchedOutcome);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="caches"></param>
        public LookupService(
            IAircraftOnlineLookupProvider provider,
            IEnumerable<IAircraftOnlineLookupCache> caches
        )
        {
            _Provider = provider;
            _Caches = caches.ToArray();
            _Timer = new(Timer_Ticked, null, dueTime: 100, period: Timeout.Infinite);
        }

        /// <inheritdoc/>
        public void Lookup(Icao24 icao24)
        {
            lock(_SyncLock) {
                if(!_LookupMap.ContainsKey(icao24)) {
                    _LookupMap.Add(icao24, DateTime.UtcNow);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<LookupOutcome> LookupAsync(Icao24 icao24, CancellationToken cancellationToken)
        {
            using(var awaiter = new LookupAwaiter(this, [ icao24 ])) {
                var awaiterTask = awaiter.WaitUntilCompleted(cancellationToken);
                Lookup(icao24);
                var outcomes = await awaiterTask;
                return outcomes.FirstOrDefault();       // <-- could potentially be empty if the task is cancelled early
            }
        }

        /// <inheritdoc/>
        public void LookupMany(IEnumerable<Icao24> icao24s)
        {
            lock(_SyncLock) {
                foreach(var icao24 in icao24s) {
                    Lookup(icao24);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<BatchedLookupOutcome> LookupManyAsync(IEnumerable<Icao24> icao24s, CancellationToken cancellationToken)
        {
            using(var awaiter = new LookupAwaiter(this, icao24s)) {
                var awaiterTask = awaiter.WaitUntilCompleted(cancellationToken);
                LookupMany(icao24s);
                var outcomes = await awaiterTask;
                return new BatchedLookupOutcome(
                    outcomes.Where(r => r.Success),
                    outcomes.Where(r => !r.Success)
                );
            }
        }

        /// <inheritdoc/>
        public bool LookupNeedsRefresh(LookupOutcome lookupOutcome)
        {
            throw new NotImplementedException();
        }

        private async void Timer_Ticked(object state)
        {
            var backOff = false;

            backOff = await InitialiseProvider(backOff);

            if(_ProviderInitialised) {
                Icao24[] icao24s;
                lock(_SyncLock) {
                    icao24s = _LookupMap.Keys.ToArray();
                }

                if(icao24s.Length > 0) {
                    var batchOutcome = await ReadFromCache(icao24s);
                    backOff = await LookupMissingIcaosOnline(batchOutcome, backOff);

                    lock(_SyncLock) {
                        foreach(var outcome in batchOutcome.AllOutcomes) {
                            _LookupMap.Remove(outcome.Icao24);
                        }
                    }

                    OnLookupCompleted(batchOutcome);
                }
            }

            _TimerPauseSeconds = !backOff
                ? _Provider.MinSecondsBetweenRequests
                : Math.Min(
                      _Provider.MaxSecondsAfterFailedRequest,
                      _TimerPauseSeconds + Math.Max(1, _Provider.MinSecondsBetweenRequests)
                  );

            _Timer.Change(dueTime: _TimerPauseSeconds * 1000, period: Timeout.Infinite);
        }

        private async Task<bool> InitialiseProvider(bool backOff)
        {
            if(!_ProviderInitialised) {
                try {
                    await _Provider.InitialiseSupplierDetails(CancellationToken.None);
                    _ProviderInitialised = true;
                } catch {
                    // We don't log exceptions from the provider, they would spam the log if the user is offline.
                    backOff = true;
                }
            }

            return backOff;
        }

        private async Task<bool> LookupMissingIcaosOnline(BatchedLookupOutcome batchedOutcome, bool backOff)
        {
            if(batchedOutcome.Missing.Count > 0) {
                BatchedLookupOutcome lookupOutcome;
                try {
                    lookupOutcome = await _Provider.LookupIcaos(batchedOutcome.Missing.Select(r => r.Icao24), CancellationToken.None);
                } catch {
                    // We don't log exceptions from the provider, they would spam the log if the user goes offline
                    lookupOutcome = null;
                    backOff = true;
                }

                if(lookupOutcome != null) {
                    await WriteToCache(lookupOutcome);

                    batchedOutcome.Found.AddRange(lookupOutcome.Found);
                    batchedOutcome.Missing.Clear();
                    batchedOutcome.Missing.AddRange(lookupOutcome.Missing);
                }
            }

            return backOff;
        }

        private async Task<BatchedLookupOutcome> ReadFromCache(Icao24[] icao24s)
        {
            var result = new BatchedLookupOutcome();
            var unknownSet = new HashSet<Icao24>(icao24s);

            foreach(var cache in _Caches.Where(r => r.CanRead).OrderByDescending(r => r.ReadPriority)) {
                var cachedOutcome = await cache.Read(unknownSet);
                foreach(var found in cachedOutcome.Found) {
                    result.Found.Add(found);
                    unknownSet.Remove(found.Icao24);
                }
                if(unknownSet.Count == 0) {
                    break;
                }
            }

            foreach(var unknownIcao in unknownSet) {
                result.Missing.Add(new LookupOutcome(unknownIcao, success: false));
            }

            return result;
        }

        private async Task WriteToCache(BatchedLookupOutcome lookupOutcome)
        {
            if(lookupOutcome.Found.Count > 0 || lookupOutcome.Missing.Count > 0) {
                var alreadySaved = false;
                foreach(var cache in _Caches.Where(r => r.CanWrite).OrderByDescending(r => r.WritePriority)) {
                    await cache.Write(lookupOutcome, alreadySaved);
                    alreadySaved = true;
                }
            }
        }
    }
}
