// Copyright © 2015 onwards, Andrew Whewell
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
    /// Conversion routes for hex text to numbers.
    /// </summary>
    public static class Hex
    {
        /// <summary>
        /// Parses a hex string into a signed integer.
        /// </summary>
        /// <param name="hexText"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        /// <remarks>
        /// In testing the standard Convert.ToInt32() conversion call in the .NET Framework took about 67ms
        /// for a million valid 6 digit hex codes and 20 seconds for a million invalid hex codes (it throws
        /// exceptions which need to be caught). This version takes about 50ms for a million valid or invalid
        /// 6 digt hex codes.
        /// </remarks>
        public static int ToInteger(string hexText, int maxLength = 8)
        {
            var result = -1;

            if(hexText != null && hexText.Length > 0 && hexText.Length <= maxLength) {
                result = 0;
                for(var i = 0;i < hexText.Length;++i) {
                    var digit = -1;
                    switch(hexText[i]) {
                        case '0':   digit = 0; break;
                        case '1':   digit = 1; break;
                        case '2':   digit = 2; break;
                        case '3':   digit = 3; break;
                        case '4':   digit = 4; break;
                        case '5':   digit = 5; break;
                        case '6':   digit = 6; break;
                        case '7':   digit = 7; break;
                        case '8':   digit = 8; break;
                        case '9':   digit = 9; break;
                        case 'a':
                        case 'A':   digit = 10; break;
                        case 'b':
                        case 'B':   digit = 11; break;
                        case 'c':
                        case 'C':   digit = 12; break;
                        case 'd':
                        case 'D':   digit = 13; break;
                        case 'e':
                        case 'E':   digit = 14; break;
                        case 'f':
                        case 'F':   digit = 15; break;
                    }
                    if(digit == -1) {
                        result = -1;
                        break;
                    }
                    result <<= 4;
                    result |= digit;
                }
            }

            return result;
        }
    }
}
