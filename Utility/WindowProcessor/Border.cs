// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace WindowProcessor
{
    public static class Border
    {
        // Letters always in this order: T = Top,        M = Middle (top or bottom), B = Bottom
        //                               L = Left,       C = Centre (left or right), R = Right
        //                               1 = single,     2 = double

        // Top-left
        public const char T1L1 =    '┌';
        public const char T1L2 =    '╓';
        public const char T2L1 =    '╒';
        public const char T2L2 =    '╔';

        // Top junction
        public const char T1C1 =    '┬';
        public const char T1C2 =    '╥';
        public const char T2C1 =    '╤';
        public const char T2C2 =    '╦';

        // Top-right
        public const char T1R1 =    '┐';
        public const char T1R2 =    '╖';
        public const char T2R1 =    '╕';
        public const char T2R2 =    '╗';

        // Left junction
        public const char M1L1 =    '├';
        public const char M1L2 =    '╟';
        public const char M2L1 =    '╞';
        public const char M2L2 =    '╠';

        // Crossroads
        public const char M1C1 =    '┼';
        public const char M1C2 =    '╫';
        public const char M2C1 =    '╪';
        public const char M2C2 =    '╬';

        // Right junction
        public const char M1R1 =    '┤';
        public const char M1R2 =    '╢';
        public const char M2R1 =    '╡';
        public const char M2R2 =    '╣';

        // Bottom-left
        public const char B1L1 =    '└';
        public const char B1L2 =    '╙';
        public const char B2L1 =    '╘';
        public const char B2L2 =    '╚';

        // Bottom junction
        public const char B1C1 =    '┴';
        public const char B1C2 =    '╨';
        public const char B2C1 =    '╧';
        public const char B2C2 =    '╩';

        // Bottom-right
        public const char B1R1 =    '┘';
        public const char B1R2 =    '╜';
        public const char B2R1 =    '╛';
        public const char B2R2 =    '╝';

        public const char M1 =      '─';
        public const char M2 =      '═';
        public const char C1 =      '│';
        public const char C2 =      '║';

        public static char TopLeft(BorderOptions styles) => LineChar(styles, T1L1, T1L2, T2L1, T2L2);

        public static char TopJunction(BorderOptions styles) => LineChar(styles, T1C1, T1C2, T2C1, T2C2);

        public static char TopRight(BorderOptions styles) => LineChar(styles, T1R1, T1R2, T2R1, T2R2);

        public static char LeftJunction(BorderOptions styles) => LineChar(styles, M1L1, M1L2, M2L1, M2L2);

        public static char Crossroads(BorderOptions styles) => LineChar(styles, M1C1, M1C2, M2C1, M2C2);

        public static char RightJunction(BorderOptions styles) => LineChar(styles, M1R1, M1R2, M2R1, M2R2);

        public static char BottomLeft(BorderOptions styles) => LineChar(styles, B1L1, B1L2, B2L1, B2L2);

        public static char BottomJunction(BorderOptions styles) => LineChar(styles, B1C1, B1C2, B2C1, B2C2);

        public static char BottomRight(BorderOptions styles) => LineChar(styles, B1R1, B1R2, B2R1, B2R2);

        public static char Horizontal(BorderOptions styles) => styles.HorizontalStyle == BorderStyle.Single ? M1 : M2;

        public static char Vertical(BorderOptions styles) => styles.VerticalStyle == BorderStyle.Single ? C1 : C2;

        private static char LineChar(BorderOptions styles, char h1v1, char h1v2, char h2v1, char h2v2)
        {
            if(styles.HorizontalStyle == BorderStyle.Single && styles.VerticalStyle == BorderStyle.Single) {
                return h1v1;
            }
            if(styles.HorizontalStyle == BorderStyle.Single && styles.VerticalStyle == BorderStyle.Double) {
                return h1v2;
            }
            if(styles.HorizontalStyle == BorderStyle.Double && styles.VerticalStyle == BorderStyle.Single) {
                return h2v1;
            }
            return h2v2;
        }
    }
}
