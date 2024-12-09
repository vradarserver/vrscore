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

namespace VirtualRadar.AircraftLists
{
    /// <summary>
    /// The default implementation of <see cref="IAircraftList"/>.
    /// </summary>
    [AircraftList(typeof(AircraftListOptions))]
    public class AircraftList(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        IAircraftListOptions _Options
        #pragma warning restore IDE1006
    ) : IAircraftList
    {
        private readonly object _SyncLock = new();
        private readonly Dictionary<int, Aircraft> _AircraftById = [];
        private readonly Dictionary<Icao24, Aircraft> _AircraftByIcao24 = [];
        private long _Stamp = 0L;

        /// <inheritdoc/>
        public long Stamp => _Stamp;

        /// <inheritdoc/>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public (bool AddedAircraft, bool ChangedAircraft) ApplyMessage(TransponderMessage message)
        {
            var isNew = false;
            var changed = false;

            if(message != null) {
                lock(_SyncLock) {
                    isNew = !_AircraftById.TryGetValue(message.AircraftId, out var aircraft);
                    if(isNew) {
                        changed = true;
                        aircraft = new(message.AircraftId);
                        _AircraftById[message.AircraftId] = aircraft;
                    }

                    var originalIcao24 = aircraft.Icao24.Value;

                    changed = aircraft.CopyFromMessage(message) || changed;
                    _Stamp = Math.Max(_Stamp, aircraft.Stamp);

                    // Note that if the feed assigns the same ICAO24 to multiple aircraft then things
                    // are going to get weird. But whatever. In real life it'll only be flight sim feeds
                    // that might have multiple aircraft with the same ICAO24.
                    var aircraftIcao24 = aircraft.Icao24.Value;
                    if(originalIcao24 != aircraftIcao24) {
                        if((originalIcao24?.IsValid ?? false) && originalIcao24 > 0) {
                            _AircraftByIcao24.Remove(originalIcao24.Value);
                        }
                        if((aircraftIcao24?.IsValid ?? false) && aircraftIcao24 > 0) {
                            _AircraftByIcao24[aircraftIcao24.Value] = aircraft;
                        }
                    }
                }
            }

            return (isNew, changed);
        }

        /// <inheritdoc/>
        public bool ApplyLookup(LookupByAircraftIdOutcome lookup)
        {
            var changed = false;

            if(lookup?.Success ?? false) {
                lock(_SyncLock) {
                    if(_AircraftById.TryGetValue(lookup.AircraftId, out var aircraft)) {
                        changed = aircraft.CopyFromLookup(lookup);
                        _Stamp = Math.Max(_Stamp, aircraft.Stamp);
                    }
                }
            }

            return changed;
        }

        /// <inheritdoc/>
        public bool ApplyLookup(BatchedLookupOutcome<LookupByAircraftIdOutcome> batchedOutcome)
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
        public bool ApplyLookup(LookupByIcaoOutcome lookup)
        {
            var changed = false;

            if(lookup?.Success ?? false) {
                lock(_SyncLock) {
                    if(_AircraftByIcao24.TryGetValue(lookup.Icao24, out var aircraft)) {
                        changed = aircraft.CopyFromLookup(lookup);
                        _Stamp = Math.Max(_Stamp, aircraft.Stamp);
                    }
                }
            }

            return changed;
        }

        /// <inheritdoc/>
        public bool ApplyLookup(BatchedLookupOutcome<LookupByIcaoOutcome> batchedOutcome)
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
        public Aircraft[] ToArray(out long arrayStamp)
        {
            lock(_SyncLock) {
                arrayStamp = Stamp;
                var result = _AircraftById
                    .Values
                    .OfType<Aircraft>()
                    .Select(aircraft => aircraft.ShallowCopy())
                    .ToArray();
                return result;
            }
        }
    }
}
