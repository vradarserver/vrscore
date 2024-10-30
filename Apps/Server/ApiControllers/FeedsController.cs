// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.AspNetCore.Mvc;
using VirtualRadar.Receivers;
using VirtualRadar.WebSite;

namespace VirtualRadar.Server.ApiControllers
{
    [ApiController]
    public class FeedsController(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        IReceiverFactory _ReceiverFactory
        #pragma warning restore IDE1006
    ) : ControllerBase
    {
        /// <summary>
        /// Returns a list of every public facing feed.
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("api/3.00/feeds")]
        public FeedJson[] GetFeeds()
        {
            return _ReceiverFactory
                .Receivers
                .Select(receiver => FeedJson.FromReceiver(receiver))
                .ToArray();
        }

        /// <summary>
        /// Returns details for a single feed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("api/3.00/feeds/{id}")]
        public FeedJson GetFeed(int id)
        {
            return FeedJson.FromReceiver(
                _ReceiverFactory.FindById(id)
            );
        }

        /// <summary>
        /// Returns the polar plot for a feed.
        /// </summary>
        /// <param name="feedId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/3.00/feeds/polar-plot/{feedId}")]
        [Route("PolarPlot.json")]                       // pre-version 3 route
        public object GetPolarPlot(int feedId = -1)
        {
            // TODO
            return null;
        }
    }
}
