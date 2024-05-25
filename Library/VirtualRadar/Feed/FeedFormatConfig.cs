// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.IO;
using VirtualRadar.Message;

namespace VirtualRadar.Feed
{
    /// <summary>
    /// The base for feed format configuration objects. Each library or plugin
    /// that implements a feed format has to fill one of these in and give it to
    /// the feed format factory service on startup.
    /// </summary>
    public abstract class FeedFormatConfig
    {
        /// <summary>
        /// The prefix used by all feed formats that ship with Virtual Radar Server.
        /// </summary>
        public const string InternalIdPrefix = "vrs-";

        /// <summary>
        /// The feed format's internal ID. This is saved to configuration files
        /// so pick something that isn't going to change. This is not shown to
        /// the user. IDs that start with <see cref="InternalIdPrefix"/> are
        /// reserved for use with Virtual Radar Server.
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// Returns the name of the feed format. Defaults to <see cref="Id"/>,
        /// you probably want to return something more sensible. The internal
        /// formats will Book Title Capitalise the format name for all cultures.
        /// </summary>
        /// <param name="uiCulture"></param>
        /// <returns></returns>
        public virtual string Name(CultureInfo uiCulture) => Id;

        /// <summary>
        /// True if the feed carries representations of aircraft transponder
        /// messages. The format should implement <see cref="ITransponderMessageConverter"/>
        /// if this is true.
        /// </summary>
        public abstract bool IsTransponderFormat { get; }

        /// <summary>
        /// Returns a new instance of a stream chunker that can extract message
        /// bytes out of a stream of bytes.
        /// </summary>
        /// <returns></returns>
        public abstract StreamChunker CreateChunker();

        /// <summary>
        /// Returns the type to request from dependency injection when <see cref="IsTransponderFormat"/>
        /// is true and VRS needs an implementation of <see cref="ITransponderMessageConverter"/>
        /// for the feed. Can throw an exception if <see cref="IsTransponderFormat"/> is false.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetTransponderMessageConverterServiceType();
    }
}
