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
    /// An enumeration of the different resize modes supported by the drawing abstraction.
    /// </summary>
    public enum ResizeMode
    {
        /// <summary>
        /// Image is drawn at top-left and is just large enough to fill the new space in both axis. One axis
        /// may end up being clipped.
        /// </summary>
        Normal,

        /// <summary>
        /// As per <see cref="Normal"/> but if an axis is clipped then the top or left is offset so that the
        /// centre is still in the middle of the new stockImage.
        /// </summary>
        Centre,

        /// <summary>
        /// Image is stretched to exactly fill the width and height of the new image.
        /// </summary>
        Stretch,

        /// <summary>
        /// Image is centred within new image, keeping the same aspect ratio as the original. One axis will be
        /// the same as the new image, the other may be smaller. Unused space is filled with a brush.
        /// </summary>
        Zoom,
    }
}
