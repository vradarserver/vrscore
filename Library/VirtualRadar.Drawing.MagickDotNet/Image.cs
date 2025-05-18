// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using ImageMagick;
using ImageMagick.Drawing;
using VirtualRadar.Extensions;

namespace VirtualRadar.Drawing.MagickDotNet
{
    class Image : IImage
    {
        private readonly object _SyncLock = new();
        private static readonly DrawableStrokeColor _DrawBlack = new(MagickColors.Black);
        private static readonly DrawableStrokeWidth _Draw1px = new(1);

        /// <summary>
        /// The wrapped image.
        /// </summary>
        private readonly MagickImage _Image;

        /// <inheritdoc/>
        public Size Size { get; private set; }

        /// <inheritdoc/>
        public int Width => Size.Width;

        /// <inheritdoc/>
        public int Height => Size.Height;

        public bool IsCachedOriginal { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="isCachedOriginal"></param>
        public Image(MagickImage image, bool isCachedOriginal)
        {
            _Image = image;
            RebuildSize();
            IsCachedOriginal = isCachedOriginal;
        }

        /// <summary>
        /// Creates a blank image.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="isCachedOriginal"></param>
        public Image(int width, int height, bool isCachedOriginal) : this(new MagickImage(
            MagickColors.Transparent,
            (uint)width,
            (uint)height
        ), isCachedOriginal)
        {
        }

        /// <summary>
        /// Creates a new image from the bytes of an existing image.
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <param name="isCachedOriginal"></param>
        public Image(byte[] imageBytes, bool isCachedOriginal) : this(new MagickImage(imageBytes), isCachedOriginal)
        {
        }

        /// <summary>
        /// Implements <see cref="IGraphics.AddTextLines"/>
        /// </summary>
        /// <param name="fontFileName"></param>
        /// <param name="textLines"></param>
        /// <param name="centreText"></param>
        /// <param name="isHighDpi"></param>
        public void AddTextLines(string fontFileName, IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            AssertMutable();
            lock(_SyncLock) {
                var lines = textLines.Where(tl => tl != null).ToList();
                var lineHeight = isHighDpi ? 24f : 12f;
                var topOffset = 5f;
                var startPointSize = isHighDpi ? 20f : 10f;
                var haloMod = isHighDpi ? 8 : 4;
                var haloTrans = isHighDpi ? 0.125f : 0.25f;
                var left = centreText ? Width / 2 : 0;
                var top = (Height - topOffset) - (lines.Count * lineHeight);

                foreach(var line in lines) {
                    if(line.Length > 0) {
                        _Image.Settings.Font = System.IO.Path.GetFullPath(fontFileName);
                        var settings = new Drawables()
                            .FontPointSize(10)
                            .FillColor(MagickColors.Black)
                            .TextAlignment(centreText ? TextAlignment.Center : TextAlignment.Left)
                            .Text(left, top + lineHeight, line);
                        settings.Draw(_Image);
                    }
                    top += lineHeight;
                }
            }
        }

        /// <summary>
        /// Clones the image.
        /// </summary>
        /// <returns></returns>
        public Image Clone() => new(new MagickImage(_Image), isCachedOriginal: false);

        /// <inheritdoc/>
        public void Dispose() => _Image.Dispose();

        /// <summary>
        /// Implements <see cref="IGraphics.GetImageBytes"/>.
        /// </summary>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Implements <see cref="IGraphics.AddAltitudeStalk"/>.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="centreX"></param>
        public Image CopyWithAltitudeStalk(int height, int centreX)
        {
            var result = new Image(Width, height, isCachedOriginal: false);
            var startOfAltitudeLine = Height / 2;

            lock(_SyncLock) {
                // Draw the altitude line
                result._Image.Draw(
                    _DrawBlack, _Draw1px,
                    new DrawableLine(
                        centreX, startOfAltitudeLine,
                        centreX, height - 3
                    )
                );

                // Draw the X at the bottom of the altitude line
                result._Image.Draw(
                    _DrawBlack, _Draw1px,
                    new DrawableLine(
                        centreX - 2, height - 5,
                        centreX + 3, height - 1
                    )
                );
                result._Image.Draw(
                    _DrawBlack, _Draw1px,
                    new DrawableLine(
                        centreX - 3, height - 1,
                        centreX + 2, height - 5
                    )
                );

                // Draw this image on top of all the lines
                result._Image.Composite(_Image, 0, 0, CompositeOperator.Over);
            }

            return result;
        }

        /// <summary>
        /// Implements <see cref="IGraphics.HeightenImage"/>.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="centreVertically"></param>
        /// <returns></returns>
        public void HeightenImage(int height, bool centreVertically)
        {
            AssertMutable();
            lock(_SyncLock) {
                _Image.Extent(
                    (uint)Width,
                    (uint)height,
                    centreVertically ? Gravity.Center : Gravity.Northwest,
                    MagickColors.Transparent
                );
                RebuildSize();
            }
        }

        /// <summary>
        /// Implements <see cref="IGraphics.ResizeBitmap()"/>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="zoomBackground"></param>
        /// <param name="preferSpeedOverQuality"></param>
        public void Resize(
            int width,
            int height,
            ResizeMode mode,
            Colour zoomBackground,
            bool preferSpeedOverQuality
        )
        {
            AssertMutable();

            var newWidth = width;
            var newHeight = height;
            var left = 0;
            var top = 0;

            if(mode != ResizeMode.Stretch) {
                var widthPercent = (double)newWidth / (double)Width;
                var heightPercent = (double)newHeight / (double)Height;
                switch(mode) {
                    case ResizeMode.Zoom:
                        if(widthPercent > heightPercent) {
                            newWidth = Math.Min(newWidth, (int)(Width * heightPercent).Round(0));
                        } else if(heightPercent > widthPercent) {
                            newHeight = Math.Min(newHeight, (int)(Height * widthPercent).Round(0));
                        }
                        break;
                    case ResizeMode.Normal:
                    case ResizeMode.Centre:
                        if(widthPercent > heightPercent) {
                            newHeight = Math.Max(newHeight, (int)(Height * widthPercent).Round(0));
                        } else if(heightPercent > widthPercent) {
                            newWidth = Math.Max(newWidth, (int)(Width * heightPercent).Round(0));
                        }
                        break;
                }

                if(mode != ResizeMode.Normal) {
                    left = (width - newWidth) / 2;
                    top = (height - newHeight) / 2;
                }

                lock(_SyncLock) {
                    _Image.FilterType = preferSpeedOverQuality
                        ? FilterType.Cubic
                        : FilterType.Lanczos;
                    _Image.Resize((uint)newWidth, (uint)newHeight);
                    _Image.Extent((uint)width, (uint)height, Gravity.Northwest, zoomBackground.ToMagickColor());
                    if(left != 0 || top != 0) {
                        _Image.Shave((uint)left, (uint)top);
                    }
                    RebuildSize();
                }
            }
        }

        /// <summary>
        /// Implements <see cref="IGraphics.Rotate"/>.
        /// </summary>
        /// <param name="degrees"></param>
        public void Rotate(double degrees)
        {
            AssertMutable();
            lock(_SyncLock) {
                _Image.BackgroundColor = MagickColors.Transparent;
                _Image.Rotate(degrees);
            }
        }

        /// <summary>
        /// Implements <see cref="IGraphics.WidenImage"/>.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="centreHorizontally"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public void WidenImage(int width, bool centreHorizontally)
        {
            AssertMutable();
            lock(_SyncLock) {
                _Image.Extent(
                    (uint)width,
                    (uint)Height,
                    centreHorizontally ? Gravity.Center : Gravity.Northwest,
                    MagickColors.Transparent
                );
                RebuildSize();
            }
        }

        private void AssertMutable()
        {
            if(IsCachedOriginal) {
                throw new InvalidOperationException("An attempt was made to modify a cached original image");
            }
        }

        private void RebuildSize()
        {
            Size = new Size((int)_Image.Width, (int)_Image.Height);
        }
    }
}
