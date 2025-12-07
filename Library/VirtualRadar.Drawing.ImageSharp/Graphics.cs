// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Drawing.ImageSharp
{
    /// <inheritdoc/>
    class Graphics : IGraphics
    {
        /// <inheritdoc/>
        public IImage AddAltitudeStalk(IImage original, int height, int centreX)
        {
            var result = original;

            if(original is ImageWrapper wrapper) {
                result = wrapper.CopyWithAltitudeStalk(height, centreX);
                DisposeIfNotCachedOriginal(original);
            }

            return result;
        }

        public IImage AddTextLines(IImage image, string fontFileName, IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            return CloneOrReuse(image);
        }

        /// <inheritdoc/>
        public IImage CreateBlankImage(int width, int height, bool isCachedOriginal = false)
        {
            var result = new ImageWrapper(width, height, isCachedOriginal);
            return result;
        }

        /// <inheritdoc/>
        public IImage CreateImage(byte[] bytes, bool isCachedOriginal = false)
        {
            var result = new ImageWrapper(bytes, isCachedOriginal);
            return result;
        }

        public IImage CreateIPhoneSplash(string webSiteAddress, bool isIPad, List<string> pathParts)
        {
            return CreateBlankImage(1, 1);
        }

        /// <inheritdoc/>
        public void DisposeIfNotCachedOriginal(IImage image)
        {
            if(!image.IsCachedOriginal) {
                image.Dispose();
            }
        }

        /// <inheritdoc/>
        public byte[] GetImageBytes(IImage image, ImageFormat imageFormat)
        {
            byte[] result = null;

            if(image is ImageWrapper wrapper) {
                result = wrapper.ToBytes(imageFormat);
            }

            return result ?? [];
        }

        /// <inheritdoc/>
        public IImage HeightenImage(IImage original, int height, bool centreVertically)
        {
            var result = original;

            if(original is ImageWrapper wrapper) {
                result = wrapper.ChangeDimension(changeWidth: false, height, centreVertically);
                DisposeIfNotCachedOriginal(original);
            }

            return result;
        }

        /// <inheritdoc/>
        public IImage ResizeBitmap(IImage original, int width, int height, ResizeMode mode, Colour zoomBackground, bool preferSpeedOverQuality)
        {
            var result = original;

            if(original is ImageWrapper wrapper) {
                result = wrapper.ResizeBitmap(width, height, mode, zoomBackground, preferSpeedOverQuality);
                DisposeIfNotCachedOriginal(original);
            }

            return result;
        }

        public IImage ResizeForHiDpi(IImage original)
        {
            return CloneOrReuse(original);
        }

        /// <inheritdoc/>
        public IImage RotateImage(IImage original, double degrees)
        {
            var result = original;

            if(degrees != 0 && original is ImageWrapper wrapper) {
                result = wrapper.RotateImage((float)degrees);
                DisposeIfNotCachedOriginal(original);
            }

            return result;
        }

        /// <inheritdoc/>
        public IImage WidenImage(IImage original, int width, bool centreHorizontally)
        {
            var result = original;

            if(original is ImageWrapper wrapper) {
                result = wrapper.ChangeDimension(changeWidth: true, width, centreHorizontally);
                DisposeIfNotCachedOriginal(original);
            }

            return result;
        }

        /// <summary>
        /// If the image is a cached original then a clone is returned, otherwise the
        /// image is returned. We only need to clone a cached image once, after that
        /// we can keep reusing the same image.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private static IImage CloneOrReuse(IImage original)
        {
            var result = original;

            if(original is ImageWrapper image && image.IsCachedOriginal) {
                result = image.Clone();
            }

            return result;
        }
    }
}
