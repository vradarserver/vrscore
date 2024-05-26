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
using VirtualRadar.Message;

namespace VirtualRadar.Services.AircraftOnlineLookup
{
    record CacheRecord
    {
        [BsonId]
        public int Icao24 { get; init; }

        public bool Success { get; init; }

        public string ModelIcao { get; init; }

        public string Manufacturer { get; init; }

        public string Model { get; init; }

        public string OperatorIcao { get; init; }

        public string Operator { get; init; }

        public string Registration { get; init; }

        public string Serial { get; init; }

        public int? YearFirstFlight { get; init; }

        public DateTime UpdatedUtc { get; init; }

        [BsonCtor]
        public CacheRecord(
            int icao24,
            bool success,
            string modelIcao,
            string manufacturer,
            string model,
            string operatorIcao,
            string @operator,
            string registration,
            string serial,
            int? yearFirstFlight,
            DateTime updatedUtc
        )
        {
            Icao24 =            icao24;
            Success =           success;
            ModelIcao =         modelIcao;
            Manufacturer =      manufacturer;
            Model =             model;
            OperatorIcao =      operatorIcao;
            Operator =          @operator;
            Registration =      registration;
            Serial =            serial;
            YearFirstFlight =   yearFirstFlight;
            UpdatedUtc =        updatedUtc;
        }

        public CacheRecord() : this(
            icao24:             0,
            success:            false,
            modelIcao:          null,
            manufacturer:       null,
            model:              null,
            operatorIcao:       null,
            @operator:          null,
            registration:       null,
            serial:             null,
            yearFirstFlight:    null,
            updatedUtc:         DateTime.UtcNow
        )
        {
        }

        public CacheRecord(LookupOutcome lookup) : this(
            icao24:             lookup.Icao24,
            success:            lookup.Success,
            modelIcao:          lookup.ModelIcao,
            manufacturer:       lookup.Manufacturer,
            model:              lookup.Model,
            operatorIcao:       lookup.OperatorIcao,
            @operator:          lookup.Operator,
            registration:       lookup.Registration,
            serial:             lookup.Serial,
            yearFirstFlight:    lookup.YearFirstFlight,
            updatedUtc:         DateTime.UtcNow
        )
        {
        }

        public LookupOutcome ToLookupOutcome()
        {
            return new LookupOutcome() {
                Icao24 =            Icao24,
                Success =           Success,
                ModelIcao =         ModelIcao,
                Manufacturer =      Manufacturer,
                Model =             Model,
                OperatorIcao =      OperatorIcao,
                Operator =          Operator,
                Registration =      Registration,
                Serial =            Serial,
                YearFirstFlight =   YearFirstFlight,
                SourceAgeUtc =      UpdatedUtc,
            };
        }
    }
}