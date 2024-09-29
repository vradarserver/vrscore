// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Reflection;
using VirtualRadar.Collections;
using VirtualRadar.Extensions;
using VirtualRadar.Reflection;

namespace VirtualRadar.Feed
{
    /// <summary>
    /// Records and exposes the links between a configuration type and the feed decoder type that uses it.
    /// </summary>
    public static class FeedDecoderConfig
    {
        private readonly static object _SyncLock = new();
        private volatile static Dictionary<Type, Type> _ConfigToDecoderTypeMap = [];

        /// <summary>
        /// Finds all types that implement <see cref="ReceiveConnectorAttribute"/> and automatically
        /// register the connection between their options and their type.
        /// </summary>
        /// <param name="addToServices"></param>
        /// <param name="assembly"></param>
        public static void RegisterAssembly(Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            try {
                foreach(var typeAttr in AttributeTags.TaggedTypes<FeedDecoderAttribute>(assembly)) {
                    RegisterFeedDecoder(typeAttr.Attribute.OptionsType, typeAttr.Type);
                }
            } catch(Exception ex) {
                ex.AddStringData("Assembly", () => assembly.FullName);
                throw;
            }
        }

        /// <summary>
        /// Registers the feed decoder that should be built when the factory is given an options object
        /// of the type passed across.
        /// </summary>
        /// <param name="optionsType"></param>
        /// <param name="connectorType"></param>
        public static void RegisterFeedDecoder(Type optionsType, Type connectorType)
        {
            ArgumentNullException.ThrowIfNull(optionsType);
            ArgumentNullException.ThrowIfNull(connectorType);

            if(!typeof(IFeedDecoderOptions).IsAssignableFrom(optionsType)) {
                throw new InvalidOperationException($"{optionsType.Name} does not implement {nameof(IFeedDecoderOptions)}");
            }
            if(!typeof(IFeedDecoder).IsAssignableFrom(connectorType)) {
                throw new InvalidOperationException($"{connectorType.Name} does not implement {nameof(IFeedDecoder)}");
            }

            lock(_SyncLock) {
                var newMap = ShallowCollectionCopier.Copy(_ConfigToDecoderTypeMap);
                newMap[optionsType] = connectorType;
                _ConfigToDecoderTypeMap = newMap;
            }
        }

        /// <summary>
        /// Returns the feed decoder type for the options type passed across or null if no decoder type has
        /// been mapped to the options type.
        /// </summary>
        /// <param name="optionsType"></param>
        /// <returns></returns>
        public static Type FeedDecoderType(Type optionsType)
        {
            var map = _ConfigToDecoderTypeMap;
            map.TryGetValue(optionsType, out var result);
            return result;
        }
    }
}
