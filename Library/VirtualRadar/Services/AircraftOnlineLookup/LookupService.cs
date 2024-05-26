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

        private readonly HashSet<Icao24> _AwaitingLookup = new();

        private DateTimeOffset _WaitStartedUtc;

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

        /// <inheritdoc/>
        public void Lookup(Icao24 icao24)
        {
            lock(_SyncLock) {
                _AwaitingLookup.Add(icao24);
                if(_WaitStartedUtc == default) {
                    _WaitStartedUtc = DateTime.UtcNow;
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
    }
}
