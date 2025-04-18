﻿// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using ImageMagick;

namespace VirtualRadar.Drawing.MagickDotNet
{
    class Image : IImage
    {
        private readonly object _SyncLock = new();

        /// <summary>
        /// The wrapped image.
        /// </summary>
        private readonly MagickImage _Image;

        /// <inheritdoc/>
        public Size Size { get; }

        /// <inheritdoc/>
        public int Width => Size.Width;

        /// <inheritdoc/>
        public int Height => Size.Height;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="image"></param>
        public Image(MagickImage image)
        {
            _Image = image;
            Size = new Size((int)image.Width, (int)image.Height);
        }

        /// <summary>
        /// Creates a blank image.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Image(int width, int height) : this(new MagickImage(
            MagickColors.Transparent,
            (uint)width,
            (uint)height
        ))
        {
        }

        /// <summary>
        /// Creates a new image from the bytes of an existing image.
        /// </summary>
        /// <param name="imageBytes"></param>
        public Image(byte[] imageBytes) : this(new MagickImage(imageBytes))
        {
        }

        public IImage AddTextLines(IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        IImage IImage.Clone() => ((Image)this).Clone();

        /// <summary>
        /// Clones the image.
        /// </summary>
        /// <returns></returns>
        public Image Clone() => new(new MagickImage(_Image));

        public void Dispose()
        {
            _Image.Dispose();
        }

        public byte[] GetImageBytes(ImageFormat imageFormat)
        {
            var magickFormat = imageFormat.ToMagickFormat();
            lock(_SyncLock) {
                var imageBytes = _Image.ToByteArray(magickFormat);
                var result = new byte[imageBytes.Length];
                Array.Copy(imageBytes, result, result.Length);
                return result;
            }
        }

        public IImage HeightenImage(int height, bool centreVertically)
        {
            throw new NotImplementedException();
        }

        public IImage Resize(int width, int height, ResizeMode mode = ResizeMode.Normal, IBrush zoomBackground = null, bool preferSpeedOverQuality = true)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IImage Rotate(double degrees)
        {
            var result = Clone();
            result._Image.Rotate(degrees);
            return result;
        }

        public IImage WidenImage(int width, bool centreHorizontally)
        {
            throw new NotImplementedException();
        }
    }
}
