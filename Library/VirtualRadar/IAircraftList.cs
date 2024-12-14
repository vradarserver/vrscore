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
    /// <summary>
    /// The interface for all objects that can maintain a list of aircraft.
    /// </summary>
    [Lifetime(Lifetime.Transient)]
    public interface IAircraftList : IAsyncDisposable
    {
        /// <summary>
        /// Gets the highest stamp seen on any aircraft. See <see cref="PostOffice"/> for rules regarding
        /// stamps. If the stamp remains constant then the list has not changed since the last time you
        /// looked.
        /// </summary>
        long Stamp { get; }

        /// <summary>
        /// Applies the content of a single transponder message to the state held for the
        /// aircraft in the list. Adds the aircraft if it does not already exist.
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>A tuple where the first item indicates whether the message created a
        /// new aircraft record for the list and the second item indicates whether the message
        /// changed the aircraft record. A new aircraft will always be considered changed.</remarks>
        (bool AddedAircraft, bool ChangedAircraft) ApplyMessage(TransponderMessage message);

        /// <summary>
        /// Applies the outcome of a successful lookup to the state held for the aircraft in
        /// the list. Ignored if the lookup failed, or if it is for an aircraft that is not
        /// in the list.
        /// </summary>
        /// <param name="lookup"></param>
        /// <returns>True if the lookup changed an aircraft record.</returns>
        bool ApplyLookup(LookupByAircraftIdOutcome lookup);

        /// <summary>
        /// Applies the outcome of a successful lookup to the state held for the aircraft in
        /// the list. Ignores failed outcomes and outcomes for aircraft not in the list.
        /// </summary>
        /// <param name="batchedOutcome"></param>
        /// <returns>True if at least one aircraft was changed.</returns>
        bool ApplyLookup(BatchedLookupOutcome<LookupByAircraftIdOutcome> batchedOutcome);

        /// <summary>
        /// Applies the outcome of a successful lookup to the state held for the aircraft in
        /// the list. Ignored if the lookup failed, or if it is for an aircraft that is not
        /// in the list.
        /// </summary>
        /// <param name="lookup"></param>
        /// <returns>True if the lookup changed an aircraft record.</returns>
        bool ApplyLookup(LookupByIcaoOutcome lookup);

        /// <summary>
        /// Applies the outcome of a successful lookup to the state held for the aircraft in
        /// the list. Ignores failed outcomes and outcomes for aircraft not in the list.
        /// </summary>
        /// <param name="batchedOutcome"></param>
        /// <returns>True if at least one aircraft was changed.</returns>
        bool ApplyLookup(BatchedLookupOutcome<LookupByIcaoOutcome> batchedOutcome);

        /// <summary>
        /// Returns a copy of the contents of the aircraft list.
        /// </summary>
        /// <param name="arrayStamp">
        /// The value of <see cref="Stamp"/> at the point where the array is built.
        /// </param>
        /// <param name="applyDisplayTimeout">
        /// Controls whether aircraft that have not sent a message within the display timeout period are
        /// returned.
        /// </param>
        /// <returns></returns>
        Aircraft[] ToArray(out long arrayStamp, bool applyDisplayTimeout);

        /// <summary>
        /// Returns a copy of the aircraft whose ID is passed across, or null if the aircraft does not exist
        /// in the list.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <returns></returns>
        Aircraft FindAircraft(int aircraftId);
    }
}
