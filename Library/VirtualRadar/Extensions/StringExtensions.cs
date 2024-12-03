// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Extensions
{
    /// <summary>
    /// Extensions to the string type.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// A collection of all acceptable line endings.
        /// </summary>
        public static readonly string[] AllLineEndings = [ "\r\n", "\n", ];

        /// <summary>
        /// A collection of all line end characters.
        /// </summary>
        public static readonly char[] AllLineEndingCharacters = [ '\r', '\n', ];

        /// <summary>
        /// Both ASCII whitespace characters.
        /// </summary>
        public static readonly char[] AllAsciiWhiteSpaceCharacters = [ ' ', '\t', ];

        /// <summary>
        /// Truncates the string to the length passed across.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string TruncateAt(this string text, int length)
        {
            return text?.Length > length
                ? text[..length]
                : text;
        }

        /// <summary>
        /// Returns the string split into lines using either Windows or Unix line endings.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lineEndings">The acceptable line endings. Defaults to <see cref="AllLineEndings"/>.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string[] SplitIntoLines(this string text, string[] lineEndings = null, StringSplitOptions options = StringSplitOptions.None)
        {
            return (text ?? "").Split(
                lineEndings ?? AllLineEndings,
                options
            );
        }

        /// <summary>
        /// The string with everything except ASCII alphanumerics stripped out.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AsciiAlphanumeric(this string text)
        {
            var result = text;

            if(!String.IsNullOrEmpty(result)) {
                var buffer = new StringBuilder();
                for(var idx = 0;idx < result.Length;++idx) {
                    var ch = result[idx];
                    if(    (ch >= '0' && ch <= '9')
                        || (ch >= 'A' && ch <= 'Z')
                        || (ch >= 'a' && ch <= 'z')
                    ) {
                        buffer.Append(ch);
                    }
                }
                result = buffer.ToString();
            }

            return result;
        }
    }
}
