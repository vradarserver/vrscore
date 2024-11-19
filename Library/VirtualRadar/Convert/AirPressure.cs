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
    /// Air pressure unit conversions.
    /// </summary>
    public static class AirPressure
    {
        /// <summary>
        /// Converts between air pressure units.
        /// </summary>
        /// <param name="pressure"></param>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static float FromTo(float pressure, AirPressureUnit fromUnit, AirPressureUnit toUnit)
        {
            var result = pressure;

            if(fromUnit != toUnit) {
                switch(fromUnit) {
                    case AirPressureUnit.InchesMercury:
                        switch(toUnit) {
                            case AirPressureUnit.Millibar:              result /= 0.0295301F; break;
                            case AirPressureUnit.MillimetresMercury:    result *= 25.4F; break; 
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case AirPressureUnit.Millibar:
                        switch(toUnit) {
                            case AirPressureUnit.InchesMercury:         result *= 0.0295301F; break;
                            case AirPressureUnit.MillimetresMercury:    result *= 0.750061561303F; break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case AirPressureUnit.MillimetresMercury:
                        switch(toUnit) {
                            case AirPressureUnit.InchesMercury:         result /= 25.4F; break;
                            case AirPressureUnit.Millibar:              result /= 0.750061561303F; break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return result;
        }
    }
}
