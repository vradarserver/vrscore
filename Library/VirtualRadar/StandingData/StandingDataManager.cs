// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.StandingData
{
    /// <summary>
    /// Default implementation of <see cref="IStandingDataManager"/>.
    /// </summary>
    /// <param name="_Repository"></param>
    /// <param name="_Overrides"></param>
    class StandingDataManager(
        IStandingDataRepository _Repository,
        IStandingDataOverridesRepository _Overrides
    ) : IStandingDataManager
    {
        /// <inheritdoc/>
        public string RouteStatus => _Repository.Route_GetStatus();

        /// <inheritdoc/>
        public AircraftType FindAircraftType(string type) => _Repository.AircraftType_GetByCode(type);

        /// <inheritdoc/>
        public IReadOnlyList<Airline> FindAirlinesForCode(string code) => _Repository.Airlines_GetByCode(code);

        /// <inheritdoc/>
        public Airport FindAirportForCode(string code) => _Repository.Airport_GetByCode(code);

        /// <inheritdoc/>
        public CodeBlock FindCodeBlock(Icao24 icao24)
        {
            return _Overrides.CodeBlockOverrideFor(icao24)
                ?? _Repository.CodeBlock_GetForIcao24(icao24);
        }

        /// <inheritdoc/>
        public Route FindRoute(string callsign) => _Repository.Route_GetForCallsign(callsign);
    }
}
