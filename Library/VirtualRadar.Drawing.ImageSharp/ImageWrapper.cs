// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace VirtualRadar.Drawing.ImageSharp
{
    /// <summary>
    /// Implements <see cref="IImage"/> using ImageSharp's image objects.
    /// </summary>
    class ImageWrapper : IImage
    {
        private Image<Rgba32> _Native;

        /// <inheritdoc/>
        public Size Size => _Native?.Size.ToVrs() ?? Size.Empty;

        /// <inheritdoc/>
        public int Width => _Native?.Size.Width ?? 0;

        /// <inheritdoc/>
        public int Height => _Native?.Size.Height ?? 0;

        /// <inheritdoc/>
        public bool IsCachedOriginal { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="native"></param>
        /// <param name="isCachedOriginal"></param>
        public ImageWrapper(Image<Rgba32> native, bool isCachedOriginal)
        {
            _Native = native;
            IsCachedOriginal = isCachedOriginal;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="isCachedOriginal"></param>
        public ImageWrapper(int width, int height, bool isCachedOriginal)
        {
            _Native = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));
            IsCachedOriginal = isCachedOriginal;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isCachedOriginal"></param>
        public ImageWrapper(byte[] content, bool isCachedOriginal)
        {
            _Native = Image.Load<Rgba32>(content);
            IsCachedOriginal = isCachedOriginal;
        }

        /// <inheritdoc/>
        ~ImageWrapper() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _Native?.Dispose();
                _Native = null;
            }
        }

        /// <summary>
        /// Creates a deep clone of this image. The clone is never a cached original.
        /// </summary>
        /// <returns></returns>
        public ImageWrapper Clone()
        {
            var newNative = _Native?.Clone();
            return newNative == null
                ? null
                : new ImageWrapper(newNative, isCachedOriginal: false);
        }

        /// <summary>
        /// Returns the image content as per the image format passed across.
        /// </summary>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public byte[] ToBytes(ImageFormat imageFormat)
        {
            byte[] result = null;

            if(_Native != null) {
                using(var stream = new MemoryStream()) {
                    switch(imageFormat) {
                        case ImageFormat.Bmp:
                            _Native.SaveAsBmp(stream);
                            break;
                        case ImageFormat.Gif:
                            _Native.SaveAsGif(stream);
                            break;
                        case ImageFormat.Jpeg:
                            _Native.SaveAsJpeg(stream);
                            break;
                        case ImageFormat.Png:
                            var encoder = new PngEncoder() {
                                TransparentColorMode = PngTransparentColorMode.Preserve,
                            };
                            _Native.SaveAsPng(stream, encoder);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    stream.Flush();
                    result = stream.ToArray();
                }
            }

            return result ?? [];
        }
    }
}
