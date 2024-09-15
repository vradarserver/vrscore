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

namespace VirtualRadar.Reflection
{
    /// <summary>
    /// Finds things tagged with attributes.
    /// </summary>
    public static class AttributeTags
    {
        /// <summary>
        /// Finds all types exported by the <paramref name="assembly"/> tagged with the <typeparamref
        /// name="T"/> attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <returns>A tuple of extracted types and the attribute they were tagged with.</returns>
        public static (Type Type, T Attribute)[] TaggedTypes<T>(Assembly assembly)
            where T: Attribute
        {
            ArgumentNullException.ThrowIfNull(assembly);
            return ExtractTaggedTypesFrom<T>(assembly)
                .ToArray();
        }

        /// <summary>
        /// Finds all types exported by any loaded assembly tagged with the <typeparamref name="T"/>
        /// attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static (Type Type, T Attribute)[] AllTaggedTypes<T>()
            where T: Attribute
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => ExtractTaggedTypesFrom<T>(assembly))
                .ToArray();
        }

        private static IEnumerable<(Type Type, T Attribute)> ExtractTaggedTypesFrom<T>(Assembly assembly)
            where T: Attribute
        {
            foreach(var type in assembly.GetTypes()) {
                var attribute = type.GetCustomAttribute<T>();
                if(attribute != null) {
                    yield return new(type, attribute);
                }
            }
        }
    }
}
