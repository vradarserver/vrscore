// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Convert
{
    /// <summary>
    /// Converts distances.
    /// </summary>
    public static class Distance
    {
        /// <summary>
        /// Converts between units of distance.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static double FromTo(double distance, DistanceUnit fromUnit, DistanceUnit toUnit)
        {
            switch(fromUnit) {
                case DistanceUnit.Kilometres:
                    switch(toUnit) {
                        case DistanceUnit.Kilometres:               return distance;
                        case DistanceUnit.Miles:                    return distance * 0.621371192;
                        case DistanceUnit.NauticalMiles:            return distance * 0.539956803;
                        default:
                            throw new NotImplementedException();
                    }
                case DistanceUnit.Miles:
                    switch(toUnit) {
                        case DistanceUnit.Kilometres:               return distance * 1.609344;
                        case DistanceUnit.Miles:                    return distance;
                        case DistanceUnit.NauticalMiles:            return distance * 0.868976;
                        default:
                            throw new NotImplementedException();
                    }
                case DistanceUnit.NauticalMiles:
                    switch(toUnit) {
                        case DistanceUnit.Kilometres:               return distance * 1.852;
                        case DistanceUnit.Miles:                    return distance * 1.15078;
                        case DistanceUnit.NauticalMiles:            return distance;
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
