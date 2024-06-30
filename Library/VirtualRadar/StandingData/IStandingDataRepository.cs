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
    /// The interface for a repository that can fetch standing data records.
    /// </summary>
    [Lifetime(Lifetime.Singleton)]
    public interface IStandingDataRepository
    {
        /// <summary>
        /// Retrieves the airlines matching the ICAO or IATA code passed across. Returns an empty collection
        /// if no such airlines exist.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        IReadOnlyList<Airline> Airlines_GetByCode(string code);
    /*
        /// <summary>
        /// Retrieves the aicraft type record for the ICAO8643 code passed across. Returns null if no such
        /// aircraft type exists.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        AircraftType AircraftType_GetByCode(string code);

        /// <summary>
        /// Retrieves the airport matching the ICAO or IATA code passed across. Returns null if no such
        /// airport exists.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Airport Airport_GetByCode(string code);

        /// <summary>
        /// Retrieves an unsorted collection of all code blocks.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<CodeBlock> CodeBlock_GetAll();

        /// <summary>
        /// Retrieves the code block matching the ICAO24 passed across. Returns the code block for unknown
        /// codes if the ICAO24 is not within a block known to be allocated by ICAO.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        CodeBlock CodeBlock_GetForIcao24(Icao24 icao24);

        /// <summary>
        /// Retrieves the route stored against the callsign or null if no such route exists.
        /// </summary>
        /// <param name="callsign"></param>
        /// <returns></returns>
        Route Route_GetForCallsign(string callsign);
    */
    }
}
