// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.AircraftHistory
{
    public class AircraftHistorySnapshot
    {
        private List<ChangeSet> _ChangeSets = [];
        private bool[] _IsFieldEstablished = new bool[(int)AircraftHistoryField.CountFields];

        public int Id { get; }

        public IReadOnlyList<ChangeSet> ChangeSets => _ChangeSets;

        public AircraftHistorySnapshot(int aircraftId)
        {
            Id = aircraftId;
        }

        public void CopyFrom(AircraftHistorySnapshot source)
        {
            if(source.Id != Id) {
                throw new InvalidOperationException($"Cannot copy history from {source.Id} to {Id}");
            }

            _ChangeSets.Clear();
            _ChangeSets.AddRange(source._ChangeSets);
            Array.Copy(source._IsFieldEstablished, _IsFieldEstablished, _IsFieldEstablished.Length);
        }

        public void AddChangeSet(ChangeSet changeSet)
        {
            if(_ChangeSets.Count > 0) {
                changeSet.EnsureLaterThan(_ChangeSets[^1].Utc);
            }
            changeSet.Lock();
            _ChangeSets.Add(changeSet);

            foreach(var change in changeSet.ChangedValues) {
                _IsFieldEstablished[change.Field.ToArrayIndex()] = true;
            }
        }

        public IReadOnlyList<ChangeSet> GetChangeSetsFromAndFor(DateTime fromUtc, params AircraftHistoryField[] fields)
        {
            return GetChangeSetsFromAndFor(
                changeSet => changeSet.Utc < fromUtc,
                fields
            );
        }

        public IReadOnlyList<ChangeSet> GetChangeSetsFromAndFor(long afterStamp, params AircraftHistoryField[] fields)
        {
            return GetChangeSetsFromAndFor(
                changeSet => changeSet.Stamp <= afterStamp,
                fields
            );
        }

        private IReadOnlyList<ChangeSet> GetChangeSetsFromAndFor(Func<ChangeSet, bool> isOutOfRange, params AircraftHistoryField[] fields)
        {
            var result = new LinkedList<ChangeSet>();
            var notEstablished = new LinkedList<AircraftHistoryField>();

            if(fields?.Length > 0) {
                foreach(var needField in fields) {
                    if(_IsFieldEstablished[needField.ToArrayIndex()]) {
                        notEstablished.AddLast(needField);
                    }
                }
            }

            for(var idx = _ChangeSets.Count - 1;idx >= 0;--idx) {
                var changeSet = _ChangeSets[idx];
                if(notEstablished.Count == 0 && isOutOfRange(changeSet)) {
                    break;
                }
                if(fields?.Length > 0 && !changeSet.ContainsChangesTo(fields)) {
                    continue;
                }
                result.AddFirst(changeSet);
                foreach(var changedValue in changeSet.ChangedValues) {
                    if(notEstablished.Contains(changedValue.Field)) {
                        notEstablished.Remove(changedValue.Field);
                    }
                }
            }

            return result.ToArray();
        }
    }
}
