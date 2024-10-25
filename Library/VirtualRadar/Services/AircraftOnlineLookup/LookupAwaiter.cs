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
    class LookupAwaiter : IDisposable
    {
        private LookupService _Service;
        private readonly HashSet<Icao24> _IdentifiersOfInterest = [];
        private readonly List<LookupByIcaoOutcome> _Outcomes = [];
        private object _SyncLock;

        public LookupAwaiter(LookupService service, IEnumerable<Icao24> identifiersOfInterest)
        {
            _SyncLock = _IdentifiersOfInterest;
            _Service = service;
            foreach(var icao24 in identifiersOfInterest) {
                _IdentifiersOfInterest.Add(icao24);
            }
            service.LookupCompleted += Service_LookupCompleted;
        }

        public void Dispose()
        {
            _Service.LookupCompleted -= Service_LookupCompleted;
            GC.SuppressFinalize(this);
        }

        public async Task<IReadOnlyList<LookupByIcaoOutcome>> WaitUntilCompleted(CancellationToken cancellationToken)
        {
            await Task.Run(() => {
                var spinWait = new SpinWait();

                var completed = false;
                while(!completed && !cancellationToken.IsCancellationRequested) {
                    lock(_SyncLock) {
                        completed = _IdentifiersOfInterest.Count == 0;
                    }
                    if(!completed) {
                        spinWait.SpinOnce();
                    }
                }
            }, cancellationToken);

            return _Outcomes;
        }

        private void Service_LookupCompleted(object sender, BatchedLookupOutcome<LookupByIcaoOutcome> e)
        {
            lock(_SyncLock) {
                foreach(var outcome in e.AllOutcomes) {
                    if(_IdentifiersOfInterest.Contains(outcome.Icao24)) {
                        _IdentifiersOfInterest.Remove(outcome.Icao24);
                        _Outcomes.Add(outcome);

                        if(_IdentifiersOfInterest.Count == 0) {
                            break;
                        }
                    }
                }
            }
        }
    }
}
