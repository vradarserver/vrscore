﻿// Copyright © 2024 onwards, Andrew Whewell
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
    /// Maintains a central stamp that only ever increments.
    /// </summary>
    /// <remarks>
    /// Many objects related to the aircraft state carry a stamp value. This is a 64-bit number that is
    /// greater than zero. A new stamp is requested when applying changes to the aircraft state. The new stamp
    /// value will always be greater than the old stamp value. All stamped objects share the same source of
    /// stamps (I.E. this static object).Therefore if you have a stamp value then you can compare it to stamps
    /// on other objects to determine which stamped values have changed since your held stamp value was
    /// established.
    /// </remarks>
    public static class PostOffice
    {
        private static long _Stamp = 0;

        /// <summary>
        /// Returns a new stamp. Each successive call to this will return a value that
        /// is higher than the last. This is thread safe.
        /// </summary>
        /// <returns></returns>
        public static long GetStamp() => Interlocked.Increment(ref _Stamp);

        /// <summary>
        /// Sets a stamp for unit testing.
        /// </summary>
        /// <param name="stamp"></param>
        public static void SetNextStampForUnitTest(long stamp)
        {
            _Stamp = stamp - 1;
        }
    }
}
