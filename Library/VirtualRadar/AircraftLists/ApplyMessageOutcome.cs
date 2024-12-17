// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.AircraftHistory;
using VirtualRadar.Message;

namespace VirtualRadar.AircraftLists
{
    /// <summary>
    /// The outcome of an <see cref="IAircraftList.ApplyMessage"/> call.
    /// </summary>
    public class ApplyMessageOutcome
    {
        /// <summary>
        /// The aircraft list that applied the message.
        /// </summary>
        public IAircraftList AircraftList { get; init; }

        /// <summary>
        /// The message that was applied to the aircraft list.
        /// </summary>
        public TransponderMessage Message { get; init; }

        /// <summary>
        /// True if the message was the first message seen for this aircraft by this list.
        /// </summary>
        public bool AddedAircraft { get; init; }

        /// <summary>
        /// The fields that changed as a result of this message getting applied.
        /// </summary>
        public ChangeSet ChangeSet { get; init; }

        /// <summary>
        /// True if at least one field changed as a result of the message.
        /// </summary>
        public bool ChangedAircraft => ChangeSet?.Changed ?? false;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ApplyMessageOutcome()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="aircraftList"></param>
        /// <param name="message"></param>
        /// <param name="addedAircraft"></param>
        /// <param name="changeSet"></param>
        public ApplyMessageOutcome(
            IAircraftList aircraftList,
            TransponderMessage message,
            bool addedAircraft,
            ChangeSet changeSet
        )
        {
            AircraftList = aircraftList;
            Message = message;
            AddedAircraft = addedAircraft;
            ChangeSet = changeSet;
        }

        public override string ToString() => Message?.ToString() ?? "";
    }
}
