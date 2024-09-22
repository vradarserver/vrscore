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

namespace VirtualRadar
{
    /// <inheritdoc/>
    class AircraftList : IAircraftList
    {
        private readonly AircraftListOptions _Options;
        private readonly object _SyncLock = new();
        private readonly Dictionary<Icao24, Aircraft> _AircraftByIcao24 = new();

        public AircraftList(AircraftListOptions options = null)
        {
            _Options = options ?? new();
        }

        /// <inheritdoc/>
        public (bool AddedAircraft, bool ChangedAircraft) ApplyMessage(TransponderMessage message)
        {
            var isNew = false;
            var changed = false;

            if(message != null) {
                lock(_SyncLock) {
                    isNew = !_AircraftByIcao24.TryGetValue(message.Icao24, out var aircraft);
                    if(isNew) {
                        changed = true;
                        aircraft = new(message.Icao24);
                        _AircraftByIcao24[message.Icao24] = aircraft;
                    }

                    changed = aircraft.CopyFromMessage(message) || changed;
                }
            }

            return (isNew, changed);
        }

        /// <inheritdoc/>
        public bool ApplyLookup(LookupOutcome lookup)
        {
            var changed = false;

            if(lookup?.Success ?? false) {
                lock(_SyncLock) {
                    if(_AircraftByIcao24.TryGetValue(lookup.Icao24, out var aircraft)) {
                        changed = aircraft.CopyFromLookup(lookup);
                    }
                }
            }

            return changed;
        }

        /// <inheritdoc/>
        public bool ApplyLookup(BatchedLookupOutcome batchedOutcome)
        {
            var changed = false;

            if(batchedOutcome?.Found.Count > 0) {
                lock(_SyncLock) {
                    foreach(var found in batchedOutcome.Found) {
                        changed = ApplyLookup(found) || changed;
                    }
                }
            }

            return changed;
        }

        /// <inheritdoc/>
        public Aircraft[] ToArray()
        {
            lock(_SyncLock) {
                var result = _AircraftByIcao24
                    .Values
                    .OfType<Aircraft>()
                    .Select(aircraft => aircraft.ShallowCopy())
                    .ToArray();
                return result;
            }
        }
    }
}
