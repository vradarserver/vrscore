// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar
{
    /// <summary>
    /// Extension methods for the <see cref="TransponderType"/> enum.
    /// </summary>
    public static class TransponderTypeExtensions
    {
        /// <summary>
        /// Returns true if the <paramref name="otherType"/> is a more specialised version of this
        /// transponder type (E.G. ADSB-2 transponders supercede ADSB-1 and MODE-S transponders).
        /// </summary>
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool IsSupercededBy(this TransponderType thisType, TransponderType otherType)
        {
            switch(thisType) {
                case TransponderType.Unknown:
                    return true;
                case TransponderType.ModeS:
                    return otherType != TransponderType.Unknown;
                case TransponderType.Adsb:
                    return otherType == TransponderType.Adsb0
                        || otherType == TransponderType.Adsb1
                        || otherType == TransponderType.Adsb2;
                case TransponderType.Adsb0:
                    return otherType == TransponderType.Adsb1
                        || otherType == TransponderType.Adsb2;
                case TransponderType.Adsb1:
                    return otherType == TransponderType.Adsb2;
                case TransponderType.Adsb2:
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
