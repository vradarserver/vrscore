// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Runtime.Serialization;

namespace VirtualRadar.Feed.Vatsim.ApiModels
{
    [DataContract]
    public class VatsimDataV3PilotRating
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "short_name")]
        public string ShortName { get; set; }

        [DataMember(Name = "long_name")]
        public string LongName { get; set; }

        public static VatsimDataV3PilotRating CopyFrom(VatsimDataV3PilotRating copyFrom)
        {
            VatsimDataV3PilotRating result = null;

            if(copyFrom != null) {
                result = new() {
                    Id =        copyFrom.Id,
                    ShortName = copyFrom.ShortName,
                    LongName =  copyFrom.LongName,
                };
            }

            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(VatsimDataV3PilotRating)} {{" +
                $" Id: {Id}" +
                $" ShortName: {ShortName}" +
                $" LongName: {LongName}" +
                " }";
        }
    }
}
