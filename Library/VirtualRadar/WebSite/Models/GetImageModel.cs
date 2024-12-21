// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Drawing;

namespace VirtualRadar.WebSite.Models
{
    /// <summary>
    /// A DTO that carries information about the request for an image.
    /// </summary>
    public class GetImageModel
    {
        /// <summary>
        /// The basic image to send back to the browser.
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// The format requested by the browser.
        /// </summary>
        public ImageFormat ImageFormat { get; set; }

        /// <summary>
        /// The file to read from the web site.
        /// </summary>
        public string WebSiteFileName { get; set; }

        /// <summary>
        /// The angle of rotation to apply to the image.
        /// </summary>
        public double? RotateDegrees { get; set; }

        /// <summary>
        /// The new width of the image.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Centres the image horizontally if the width needs to be changed, otherwise leaves
        /// the image aligned on the left edge.
        /// </summary>
        public bool CentreImageHorizontally { get; set; } = true;

        /// <summary>
        /// The new height of the image.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Centres the image vertically if the height needs to be changed, otherwise leaves
        /// the image aligned on the top edge.
        /// </summary>
        public bool CentreImageVertically { get; set; } = true;

        /// <summary>
        /// The centre pixel for those transformations that require one.
        /// </summary>
        public int? CentreX { get; set; }

        /// <summary>
        /// A value indicating that the altitude stalk should be drawn.
        /// </summary>
        public bool ShowAltitudeStalk { get; set; }

        /// <summary>
        /// The name of a file that should be used as the source for the image.
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// The standard size at which to render particular images.
        /// </summary>
        public StandardWebSiteImageSize Size { get; set; }

        /// <summary>
        /// A collection of text strings that need to be overlaid onto the image.
        /// </summary>
        public List<string> TextLines { get; } = [];

        /// <summary>
        /// Indicates that the browser is going to display the image in a space half the size of the img tag,
        /// or that the request is twice the size that it's expected to be rendered at.
        /// </summary>
        public bool IsHighDpi { get; set; }

        /// <summary>
        /// Returns true if <see cref="TextLines"/> contains something.
        /// </summary>
        public bool HasTextLines => TextLines.Count > 0 && TextLines.Any(r => r != null);

        /// <summary>
        /// True if the web image should always be fetched from disk rather than from the cache. This should
        /// always be disabled for Internet requests.
        /// </summary>
        public bool NoCache { get; set; }
    }
}
