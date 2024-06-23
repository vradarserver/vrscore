// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Runtime.CompilerServices;

namespace VirtualRadar
{
    public class OneTimeConfigurableImplementer<T> : IOneTimeConfigurable<T>
    {
        /// <summary>
        /// The type name to show in exceptions.
        /// </summary>
        public string ParentTypeName { get; }

        /// <inheritdoc/>
        public T Options { get; private set; }

        /// <summary>
        /// True if <see cref="Options"/> has been configured, false if it's the initial state.
        /// </summary>
        public bool Configured { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="parentTypeName"></param>
        public OneTimeConfigurableImplementer(
            string parentTypeName,
            T initialState = default
        )
        {
            ParentTypeName = parentTypeName;
            Options = initialState;
        }

        /// <inheritdoc/>
        public void Configure(T options)
        {
            if(Configured) {
                throw new InvalidOperationException($"You cannot reconfigre a {ParentTypeName}");
            }
            Options = options;
            Configured = true;
        }

        /// <summary>
        /// Throws an exception if <see cref="Configure"/> has not yet been called.
        /// </summary>
        /// <param name="caller"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AssertConfigured([CallerMemberName] string caller = null)
        {
            if(!Configured) {
                throw new InvalidOperationException($"You must configure a {ParentTypeName} before calling {caller}");
            }
        }
    }
}
