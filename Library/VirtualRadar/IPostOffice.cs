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
    /// Maintains a central stamp that only ever increments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Many objects related to the aircraft state carry a stamp value. This is a 64-bit number that is
    /// greater than zero. A new stamp is requested when applying changes to the aircraft state. The new stamp
    /// value will always be greater than the old stamp value. All stamped objects share the same source of
    /// stamps.Therefore if you have a stamp value then you can compare it to stamps on other objects to
    /// determine which stamped values have changed since your held stamp value was established.
    /// </para>
    /// <para>
    /// The stamp value is roughly convertable to a time. It is the number of 100ns ticks since 1-Jan-0001.
    /// However the "always increments" rule applies, so if two calls are made within 100ns of each other then
    /// the second call returns a number larger than the first, and if the real-time clock is moved backwards
    /// by the operating system the stamp value will continue to return 100ns tick values based on the
    /// original UTC clock, and could potentially end up returning thousands of stamps that would all resolve
    /// back to the same second or millisecond in time were one to work backwards from the stamp to a time.
    /// </para>
    /// <para>
    /// The purpose of basing stamp on a real-time clock is to keep it kind-of moving forwards between
    /// sessions. It won't always do this, but it'll be close enough for most purposes. It is not intended as
    /// a subtitute for recording timestamps. If you need to know the time, use <see cref="IClock"/>. You can
    /// use stamps as a clock but be aware that eventually you could lose connection to the real time clock
    /// and then thousands of your stamps will appear to be for roughly the same moment in time.
    /// </para>
    /// </remarks>
    [Lifetime(Lifetime.Singleton)]
    public interface IPostOffice
    {
        /// <summary>
        /// Returns a new stamp. Each successive call to this will return a value that is higher than the
        /// last. This is thread safe.
        /// </summary>
        /// <returns></returns>
        long GetStamp();
    }
}
