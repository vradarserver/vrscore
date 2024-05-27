// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Message;

namespace VirtualRadar
{
    /// <summary>
    /// Caches the results of online aircraft detail lookups.
    /// </summary>
    /// <remarks><para>
    /// There can be many lookup caches registered simultaneously. The lookup service will use all of them,
    /// calling them in order of priority. The caches are resolved when the service first starts, changes
    /// to the service configuration after startup are ignored. Use the <see cref="Enabled"/> flag to control
    /// dynamic caches.
    /// </para><para>
    /// The default cache has a priority of Int.MinValue for reading and writing, it is always called last. It
    /// will not save lookups that have already been saved by another cache.
    /// </para><para>
    /// The cache is expected to self-police the lifetime of cached records. There is a set of default thresholds
    /// for hit and miss limits in <see cref="VirtualRadar.Configuration.AircraftOnlineLookupCacheConfig"/>, but
    /// caches are free to impose their own thresholds if they wish.
    /// </para></remarks>
    [Lifetime(Lifetime.Singleton)]
    public interface IAircraftOnlineLookupCache
    {
        /// <summary>
        /// Gets a flag indicating that the cache is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Gets a value indicating the priority to assign to this cache. Higher priorities (which can be negative) are
        /// called before lower priorities.
        /// </summary>
        int ReadPriority { get; }

        /// <summary>
        /// True if the cache can read lookup values.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating the priority to assign to this cache. Higher priorities (which can be negative) are
        /// called before lower priorities.
        /// </summary>
        int WritePriority { get; }

        /// <summary>
        /// True if the cache can write lookup values.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Reads cached data for the ICAOs passed across. Note that if a cache with a higher priority exists
        /// then any ICAOs that it could supply will be excluded from the set passed across, but if the higher
        /// priority indicated that it had saved a miss for an ICAO then those are still passed to lower
        /// priority caches to see if one of those can fulfill it. This function will never be called while
        /// <see cref="CanRead"/> returns false.
        /// </summary>
        /// <param name="icaos"></param>
        /// <returns>
        /// Cached hits and misses. Do not return records for entries that have not been cached, I.E. if a
        /// failure has not previously been saved for an ICAO then do not return a 'Missing' entry for it.
        /// </returns>
        Task<BatchedLookupOutcome> Read(IEnumerable<Icao24> icaos);

        /// <summary>
        /// Writes successes and failures to the cache. If a cache with a higher priority exists then it will
        /// have been called first, but you will still see ALL of the cached records that it was asked to write.
        /// </summary>
        /// <param name="outcome"></param>
        /// <param name="hasBeenSavedByAnotherCache">True if a higher priority cache has already saved all of
        /// the outcomes.</param>
        /// <returns></returns>
        Task Write(BatchedLookupOutcome outcome, bool hasBeenSavedByAnotherCache);
    }
}
