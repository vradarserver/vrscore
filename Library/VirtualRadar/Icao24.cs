// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Diagnostics.CodeAnalysis;

namespace VirtualRadar
{
    /// <summary>
    /// Describes an ICAO Mode-S 24-bit aircraft identifier.
    /// </summary>
    public struct Icao24 : IComparable<Icao24>
    {
        private int _Value;

        /// <summary>
        /// Gets the default invalid value.
        /// </summary>
        public static readonly Icao24 Invalid = new(-1);

        /// <summary>
        /// Gets the minimum valid value.
        /// </summary>
        public static readonly Icao24 MinValue = new(0);

        /// <summary>
        /// Gets the maxiumum valid value.
        /// </summary>
        public static readonly Icao24 MaxValue = new(0xFFFFFF);

        /// <summary>
        /// True if the ICAO24 is valid.
        /// </summary>
        public bool IsValid => _Value >= 0 && _Value <= 0xFFFFFF;

        public Icao24(int value)
        {
            _Value = value;
        }

        /// <summary>
        /// Implicitly converts an integer into an ICAO24.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Icao24(int value) => new Icao24(value);

        /// <summary>
        /// Implicitly converts an ICAO24 into a 32-bit integer.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator int(Icao24 value) => value._Value;

        /// <inheritdoc/>
        public static bool operator==(Icao24 lhs, Icao24 rhs) => lhs._Value == rhs._Value;

        /// <inheritdoc/>
        public static bool operator!=(Icao24 lhs, Icao24 rhs) => lhs._Value != rhs._Value;

        /// <inheritdoc/>
        public static bool operator<(Icao24 lhs, Icao24 rhs) => lhs._Value < rhs._Value;

        /// <inheritdoc/>
        public static bool operator>(Icao24 lhs, Icao24 rhs) => lhs._Value > rhs._Value;

        /// <inheritdoc/>
        public static bool operator<=(Icao24 lhs, Icao24 rhs) => lhs._Value <= rhs._Value;

        /// <inheritdoc/>
        public static bool operator>=(Icao24 lhs, Icao24 rhs) => lhs._Value >= rhs._Value;

        /// <inheritdoc/>
        public override string ToString() => _Value.ToString("X6");

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object obj) => obj is Icao24 other && other._Value == _Value;

        /// <inheritdoc/>
        public override int GetHashCode() => _Value.GetHashCode();

        /// <inheritdoc/>
        public int CompareTo(Icao24 other) => _Value - other._Value;

        /// <summary>
        /// Parses the hex text into an ICAO24. Throws if the parse fails.
        /// </summary>
        /// <param name="hexText"></param>
        /// <param name="ignoreNonHexDigits"></param>
        /// <param name="truncateTooManyDigits"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Icao24 Parse(string hexText, bool ignoreNonHexDigits = false, bool truncateTooManyDigits = false)
        {
            ArgumentNullException.ThrowIfNull(hexText);
            var result = FromHexText(hexText, ignoreNonHexDigits, truncateTooManyDigits);
            if(result == -1) {
                throw new ArgumentOutOfRangeException(nameof(hexText), $"\"{hexText}\" is not a valid ICAO24");
            }

            return result;
        }

        /// <summary>
        /// Parses the hex text into an ICAO24. If the parse fails then false is returned and <paramref name="value"/>
        /// is set to <see cref="Invalid"/>.
        /// </summary>
        /// <param name="hexText"></param>
        /// <param name="value"></param>
        /// <param name="ignoreNonHexDigits"></param>
        /// <param name="truncateTooManyDigits"></param>
        /// <returns></returns>
        public static bool TryParse(string hexText, out Icao24 value, bool ignoreNonHexDigits = false, bool truncateTooManyDigits = false)
        {
            value = FromHexText(hexText, ignoreNonHexDigits, truncateTooManyDigits);
            return value.IsValid;
        }

        /// <summary>
        /// Parses a hex string into an ICAO24.
        /// </summary>
        /// <param name="hexText"></param>
        /// <param name="ignoreNonHexDigits"></param>
        /// <param name="truncateTooManyDigits"></param>
        /// <returns></returns>
        private static Icao24 FromHexText(
            string hexText,
            bool ignoreNonHexDigits,
            bool truncateTooManyDigits
        )
        {
            var result = -1;

            if(hexText != null) {
                result = 0;
                var countDigits = 0;
                for(var idx = 0;idx < hexText.Length;++idx) {
                    var digit = -1;
                    // This looks long-winded but it goes like the clappers.
                    switch(hexText[idx]) {
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
                    if(digit == -1 && !ignoreNonHexDigits) {
                        result = -1;
                        break;
                    }
                    if(digit != -1) {
                        if(++countDigits > 6) {
                            if(!truncateTooManyDigits) {
                                result = -1;
                            }
                            break;
                        }
                        result <<= 4;
                        result |= digit;
                    }
                }
            }

            return new(result);
        }
    }
}
