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
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Sets an element in the Data dictionary for the exception passed across to the value. Guaranteed
        /// not to throw operational exceptions but could potentially throw out-of-memory etc.
        /// </summary>
        /// <param name="ex">The exception whose Data dictionary is to be modified.</param>
        /// <param name="key">The string key to the entry in the Data dictionary to set.</param>
        /// <param name="value">
        /// The value to set for the <paramref name="key"/>. Null strings are coalesced to an empty string.
        /// </param>
        /// <param name="append">
        /// True if <paramref name="value"/> should be appended if it already exists. Defaults to true.
        /// </param>
        /// <param name="separator">
        /// The separator to use if <paramref name="append"/> is true. Defaults to semi-colon.
        /// </param>
        /// <param name="distinct">
        /// True to leave the existing value alone if <paramref name="append"/> is true and <paramref
        /// name="value"/> can be found within the existing value when split using <paramref
        /// name="separator"/>. Defaults to false.
        /// </param>
        public static void AddStringData(
            this Exception ex,
            string key,
            string value,
            bool append = true,
            string separator = ";",
            bool distinct = false
        )
        {
            try {
                var newValue = value ?? "";
                if(append) {
                    string oldValue = null;
                    if(ex.Data.Contains(key)) {
                        oldValue = ex.Data[key] as string;
                    }
                    if(oldValue != null) {
                        if(!distinct || !oldValue.Split(separator).Contains(newValue)) {
                            newValue = $"{oldValue}{separator}{newValue}";
                        }
                    }
                }
                ex.Data[key] = newValue;
            } catch {
                // Swallow any errors, we don't want to obscure the original error
                ;
            }
        }

        /// <summary>
        /// As per <see cref="AddStringData(Exception, string, string, bool, string, bool)"/> except this takes
        /// a function to the value and executes that function within a try/catch. Any exception thrown by the
        /// function is swallowed and the string data is not added to the exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="key"></param>
        /// <param name="valueFunc"></param>
        /// <param name="append"></param>
        /// <param name="separator"></param>
        /// <param name="distinct"></param>
        public static void AddStringData(
            this Exception ex,
            string key,
            Func<string> valueFunc,
            bool append = true,
            string separator = ";",
            bool distinct = false
        )
        {
            try {
                AddStringData(ex, key, valueFunc(), append, separator, distinct);
            } catch {
                ;
            }
        }
    }
}
