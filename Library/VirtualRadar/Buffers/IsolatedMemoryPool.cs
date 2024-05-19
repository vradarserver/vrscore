// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Buffers;

namespace VirtualRadar.Buffers
{
    /// <summary>
    /// A memory pool that you can instantiate.
    /// </summary>
    /// <remarks>
    /// See README.md for raison d'etre.
    /// </remarks>
    public class IsolatedMemoryPool : MemoryPool<byte>
    {
        // The ArrayPool that does all the work.
        private readonly ArrayPool<byte> _ArrayPool = ArrayPool<byte>.Create();

        /// <inheritdoc/>
        public override int MaxBufferSize => Array.MaxLength;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
        }

        /// <inheritdoc/>
        public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            if(minBufferSize == -1) {
                minBufferSize = 4096;
            } else if((uint)minBufferSize > MaxBufferSize) {
                throw new ArgumentOutOfRangeException(nameof(minBufferSize));
            }

            return new IsolatedMemoryOwner(
                _ArrayPool.Rent(minBufferSize),
                _ArrayPool
            );
        }
    }
}
