// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Configuration;
using VirtualRadar.Receivers;

namespace VirtualRadar.Feed.Vatsim
{
    [SettingsProvider(_ProviderName)]
    public record VatsimConnectorOptions(
        GeofenceCentreOn CentreOn,
        Location CentreOnLocation,
        int CentreOnPilotCid,
        string CentreOnAirport,
        DistanceUnit GeofenceDistanceUnit,
        double GeofenceWidth,
        double GeofenceHeight
    ) : IReceiverConnectorOptions
    {
        private const string _ProviderName = "VatsimConnector";

        public string SettingsProvider => _ProviderName;

        /// <summary>
        /// Default ctor.
        /// </summary>
        public VatsimConnectorOptions() : this(
            CentreOn:               GeofenceCentreOn.Coordinate,
            CentreOnLocation:       new(54.497989, -4.555628),
            CentreOnPilotCid:       0,
            CentreOnAirport:        null,
            GeofenceDistanceUnit:   DistanceUnit.Miles,
            GeofenceWidth:          560.0,
            GeofenceHeight:         740.0
        )
        {;}
    }
}
