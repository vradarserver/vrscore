// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtualRadar.Message;

namespace VirtualRadar.Database.EntityFramework.AircraftOnlineLookupCache.Entities
{
    class AircraftDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AircraftDetailID { get; set; }

        [MaxLength(6), Required]
        public string Icao { get; set; }

        public bool IsMissing { get; set; }

        [MaxLength(20)]
        public string Registration { get; set; }

        [MaxLength(200)]
        public string Country { get; set; }

        [MaxLength(200)]
        public string Manufacturer { get; set; }

        [MaxLength(200)]
        public string Model { get; set; }

        [MaxLength(10)]
        public string ModelIcao { get; set; }

        [MaxLength(200)]
        public string Operator { get; set; }

        [MaxLength(3)]
        public string OperatorIcao { get; set; }

        [MaxLength(80)]
        public string Serial { get; set; }

        public int? YearBuilt { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

        public void CopyFrom(LookupByIcaoOutcome lookupOutcome, DateTime utcNow)
        {
            Icao =          lookupOutcome.Icao24.ToString();
            IsMissing =     !lookupOutcome.Success;
            Registration =  lookupOutcome.Registration;
            Country =       lookupOutcome.Country;
            Manufacturer =  lookupOutcome.Manufacturer;
            Model =         lookupOutcome.Model;
            ModelIcao =     lookupOutcome.ModelIcao;
            Operator =      lookupOutcome.Operator;
            OperatorIcao =  lookupOutcome.OperatorIcao;
            Serial =        lookupOutcome.Serial;
            YearBuilt =     lookupOutcome.YearBuilt;

            UpdatedUtc = utcNow;
            if(CreatedUtc == default) {
                CreatedUtc = utcNow;
            }
        }

        public LookupByIcaoOutcome ToLookupByIcaoOutcome()
        {
            var result = new LookupByIcaoOutcome() {
                Icao24 =        Icao24.Parse(Icao),
                Success =       !IsMissing,
                SourceAgeUtc =  DateTime.SpecifyKind(UpdatedUtc, DateTimeKind.Utc),
            };
            if(result.Success) {
                result.Country =            Country;
                result.Manufacturer =       Manufacturer;
                result.Model =              Model;
                result.ModelIcao =          ModelIcao;
                result.Operator =           Operator;
                result.OperatorIcao =       OperatorIcao;
                result.Registration =       Registration;
                result.Serial =             Serial;
                result.YearBuilt =    YearBuilt;
            }

            return result;
        }
   }
}
