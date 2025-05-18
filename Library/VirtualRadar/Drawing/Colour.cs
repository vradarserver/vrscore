// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Drawing
{
    /// <summary>
    /// Describes an RGB colour and an alpha channel.
    /// </summary>
    public readonly struct Colour
    {
        public const byte TransparentAlpha = 0;

        public const byte OpaqueAlpha = 255;

        public static readonly Colour Black = new(0, 0, 0);

        public static readonly Colour Transparent = new(0, 0, 0, TransparentAlpha);

        public static readonly Colour White = new(0xff, 0xff, 0xff);

        public byte R { get; init; }

        public byte G { get; init; }

        public byte B { get; init; }

        public byte A { get; init; }

        public byte Red => R;

        public byte Green => G;

        public byte Blue => B;

        public byte Alpha => A;

        public static bool operator==(Colour lhs, Colour rhs)
        {
            return lhs.R == rhs.R
                && lhs.G == rhs.G
                && lhs.B == rhs.B
                && lhs.A == rhs.A;
        }

        public static bool operator!=(Colour lhs, Colour rhs)
        {
            return lhs.R != rhs.R
                || lhs.G != rhs.G
                || lhs.B != rhs.B
                || lhs.A != rhs.A;
        }

        public Colour(byte red, byte green, byte blue, byte alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        public Colour(byte red, byte green, byte blue) : this(red, green, blue, OpaqueAlpha)
        {
        }

        public Colour() : this(0, 0, 0, OpaqueAlpha)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => ToCss(showAlpha: true, preferShortForm: true);

        /// <summary>
        /// Returns the colour in CSS format.
        /// </summary>
        /// <param name="showAlpha"></param>
        /// <param name="preferShortForm"></param>
        /// <param name="includePrefix"></param>
        /// <returns></returns>
        public string ToCss(bool showAlpha, bool preferShortForm, bool includePrefix = true)
        {
            var prefix = includePrefix ? "#" : "";

            return preferShortForm && CanBeExpressedAsShortFormCss(showAlpha)
                ? showAlpha
                    ? $"{prefix}{Red % 0x10:X1}{Green % 0x10:X1}{Blue % 0x10:X1}{Alpha % 0x10:X1}"
                    : $"{prefix}{Red % 0x10:X1}{Green % 0x10:X1}{Blue % 0x10:X1}"
                : showAlpha
                    ? $"{prefix}{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}"
                    : $"{prefix}{Red:X2}{Green:X2}{Blue:X2}";
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Colour other && this == other;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(R, G, B, A);

        /// <summary>
        /// Returns true if the colour can be expressed as in short-form CSS notation (I.E. the top and bottom
        /// nibbles are the same value for each colour).
        /// </summary>
        /// <param name="includeAlpha">True to include the alpha channel when comparing nibbles.</param>
        /// <returns></returns>
        public bool CanBeExpressedAsShortFormCss(bool includeAlpha)
        {
            static bool nibblesMatch(byte x) => ((x & 0xf0) >> 4) == (x & 0x0f);

            var result = nibblesMatch(R) && nibblesMatch(G) && nibblesMatch(B);
            if(result && includeAlpha) {
                result = nibblesMatch(A);
            }

            return result;
        }
    }
}
