﻿// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Drawing
{
    /// <summary>
    /// Describes an immutable image in memory. This has no operations on it, you need to go through
    /// <see cref="IGraphics"/> - that way if the underlying graphics subsystem is not threadsafe then
    /// <see cref="IGraphics"/> can synchronise calls to it.
    /// </summary>
    public interface IImage : IDisposable
    {
        /// <summary>
        /// Gets the dimensions of the image.
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Shorthand for <see cref="Size.Width"/>.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Shorthand for <see cref="Size.Height"/>.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// False if this is a cached original and therefore (a) cannot be disposed and (b) cannot be altered.
        /// </summary>
        bool IsCachedOriginal { get; }
    }
}
