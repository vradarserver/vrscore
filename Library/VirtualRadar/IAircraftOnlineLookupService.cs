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
    /// Looks up aircraft details from the standing data site.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementations are expected to batch requests to try to cut down on the load on the standing data
    /// site. They are also expected to queue requests for up to 30 minutes if the site is down.
    /// </para>
    /// <para>
    /// This means that there can be a significant delay between requesting the details for an aircraft and
    /// the outcome being returned. The service takes two approaches to this problem.
    /// </para>
    /// <para>
    /// First, it exposes an event that is raised whenever a lookup completes. This event is raised for
    /// lookups satisfied from the standing data site or the cache.
    /// </para>
    /// <para>
    /// Hooking event handlers on singletons and then forgetting to unhook yourself before you die is an easy
    /// way to leak memory. This problem can become exacerbated by the use of Dependency Injection because
    /// that takes control over the disposal of an object away from the code. So, as an alternative there are
    /// async versions of the lookup methods that code can block on until they return. But be aware - that
    /// block can be up to half an hour!
    /// </para>
    /// </remarks>
    [Lifetime(Lifetime.Singleton)]
    public interface IAircraftOnlineLookupService
    {
        /// <summary>
        /// Raised when a bunch of lookup outcomes is known. If you request more than one ICAO24 lookup at
        /// once then do not expect a single batch to contain all of the outcomes, or for a single batch to
        /// only contain the ICAOs you requested. Likewise if you request a single ICAO24 then it might appear
        /// in with a bunch of other outcomes. There is no relationship between the grouping of requests and
        /// the grouping of outcomes.
        /// </summary>
        event EventHandler<BatchedLookupOutcome<LookupByIcaoOutcome>> LookupCompleted;

        /// <summary>
        /// Requests a lookup for the details of a single aircraft. Hook <see cref="LookupCompleted"/> and
        /// monitor it for the outcome. It may be half an hour before you get an outcome.
        /// </summary>
        /// <param name="icao24"></param>
        void Lookup(Icao24 icao24);

        /// <summary>
        /// Returns the details of a single aircraft. This might not return for up to half an hour. The
        /// lookup outcome will also be raised on <see cref="LookupCompleted"/>.
        /// </summary>
        /// <param name="icao24"></param>
        /// <param name=""></param>
        /// <returns>The outcome or null if the task was cancelled early.</returns>
        Task<LookupOutcome> LookupAsync(Icao24 icao24, CancellationToken cancellationToken);

        /// <summary>
        /// Request the lookup of many aircraft. Hook <see cref="LookupCompleted"/> and monitor it for the
        /// outcomes. It may be half an hour before you get an outcome.
        /// </summary>
        /// <param name="icao24s"></param>
        void LookupMany(IEnumerable<Icao24> icao24s);

        /// <summary>
        /// Request the lookup of many aircraft. This might not return for up to half an hour. The lookup
        /// outcomes will also be raised on <see cref="LookupCompleted"/>.
        /// </summary>
        /// <param name="icao24s"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The outcomes. This may be incomplete if the task was cancelled early.</returns>
        Task<BatchedLookupOutcome<LookupByIcaoOutcome>> LookupManyAsync(
            IEnumerable<Icao24> icao24s,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// The online service dynamically sets thresholds on when an aircraft's lookup details can be
        /// considered stale and candidates for another lookup. This call uses those dynamic thresholds
        /// to determine whether lookup passed across qualifies for a refresh.
        /// </summary>
        /// <remarks>
        /// Note that all details returned by the service will have been automatically refetched if the
        /// cached verison was out of date. This is not a function that applications need to bother with.
        /// </remarks>
        /// <param name="lookupOutcome"></param>
        /// <returns></returns>
        bool LookupNeedsRefresh(LookupOutcome lookupOutcome);
    }
}
