// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using ISProcessing = SixLabors.ImageSharp.Processing;

namespace VirtualRadar.Drawing.ImageSharp
{
    /// <summary>
    /// Implements <see cref="IImage"/> using ImageSharp's image objects.
    /// </summary>
    class ImageWrapper : IImage
    {
        private Image<Rgba32>? _Native;

        private Image<Rgba32> Safe_Native => _Native ?? throw new ObjectDisposedException(nameof(ImageWrapper));

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
        public ImageWrapper? Clone()
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
            byte[]? result = null;

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
                            _Native.SaveAsPng(stream);
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

        /// <summary>
        /// Implements <see cref="IGraphics.AddAltitudeStalk"/>.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="centreX"></param>
        public ImageWrapper CopyWithAltitudeStalk(int height, int centreX)
        {
            var result = new ImageWrapper(Width, height, isCachedOriginal: false);
            var startOfAltitudeLine = Height / 2;

            result.Safe_Native.Mutate(context => context
                // Draw the altitude line
                .DrawLine(
                    Color.Black,
                    1F,
                    new PointF(centreX, startOfAltitudeLine),
                    new PointF(centreX, height - 3)
                )

                // Draw the X at the bottom of the altitude line
                .DrawLine(
                    Color.Black,
                    1F,
                    new PointF(centreX - 2, height - 5),
                    new PointF(centreX + 3, height - 1)
                )
                .DrawLine(
                    Color.Black,
                    1F,
                    new PointF(centreX - 3, height - 1),
                    new PointF(centreX + 2, height - 5)
                )

                // Draw this image on top of all the lines
                .DrawImage(Safe_Native, 1F)
            );

            return result;
        }

        /// <summary>
        /// Implements <see cref="IGraphics.ResizeBitmap"/>.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="zoomBackground"></param>
        /// <param name="preferSpeedOverQuality"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ImageWrapper ResizeBitmap(int width, int height, ResizeMode mode, Colour zoomBackground, bool preferSpeedOverQuality)
        {
            ImageWrapper result;

            switch(mode) {
                case ResizeMode.Zoom:
                    result = ResizeZoom(width, height, zoomBackground);
                    break;
                default:
                    result = Resize(mode, width, height);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Performs a zoom resize.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="padBrush"></param>
        /// <returns></returns>
        private ImageWrapper ResizeZoom(int width, int height, Colour zoomBackground)
        {
            var resizeWidth = width;
            var resizeHeight = height;
            var widthPercent = (double)width / (double)Safe_Native.Width;
            var heightPercent = (double)height / (double)Safe_Native.Height;
            if(widthPercent > heightPercent)        resizeWidth = Math.Min(width, (int)(((double)Safe_Native.Width * heightPercent) + 0.5));
            else if(heightPercent > widthPercent)   resizeHeight = Math.Min(height, (int)(((double)Safe_Native.Height * widthPercent) + 0.5));

            var left = (width - resizeWidth) / 2;
            var top = (height - resizeHeight) / 2;

            var result = new ImageWrapper(Width, height, isCachedOriginal: false);

            using(var resizedClone = Safe_Native.Clone(context => context.Resize(resizeWidth, resizeHeight, new NearestNeighborResampler()))) {
                result.Safe_Native.Mutate(context => context
                    .Fill(zoomBackground.ToImageSharp())
                    .DrawImage(resizedClone, new Point(left, top), 1F)
                );
            }

            return result;
        }

        /// <summary>
        /// Implements all resize methods except zoom.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private ImageWrapper Resize(ResizeMode mode, int width, int height)
        {
            var options = new ResizeOptions() {
                Mode =      ISProcessing.ResizeMode.Crop,
                Position =  AnchorPositionMode.TopLeft,
                Size =      new SixLabors.ImageSharp.Size(width, height),
                Sampler =   new NearestNeighborResampler(),
            };

            switch(mode) {
                case ResizeMode.Centre:
                    options.Position = AnchorPositionMode.Center;
                    break;
                case ResizeMode.Stretch:
                    options.Mode = ISProcessing.ResizeMode.Stretch;
                    break;
                case ResizeMode.Normal:
                    break;
                default:
                    throw new NotImplementedException();
            }

            var clone = Safe_Native.Clone(context => context.Resize(options));
            return new ImageWrapper(clone, isCachedOriginal: false);
        }

        /// <summary>
        /// Returns a copy of the image with the size altered in one dimension.
        /// </summary>
        /// <param name="changeWidth"></param>
        /// <param name="newValue"></param>
        /// <param name="centre"></param>
        /// <returns></returns>
        public ImageWrapper ChangeDimension(bool changeWidth, int newValue, bool centre)
        {
            var width = changeWidth ? newValue : Safe_Native.Width;
            var height = changeWidth ? Safe_Native.Height : newValue;
            var x = 0;
            var y = 0;

            if(centre) {
                int centredValue(int oldValue)
                {
                    var centredFloat = ((float)newValue - (float)oldValue) / 2F;
                    centredFloat += 0.5F;
                    return (int)centredFloat;
                }

                if(changeWidth) {
                    x = centredValue(Safe_Native.Width);
                } else {
                    y = centredValue(Safe_Native.Height);
                }
            }

            var result = new ImageWrapper(width, height, isCachedOriginal: false);
            result.Safe_Native.Mutate(context => context.DrawImage(
                Safe_Native,
                new Point(x, y),
                1F
            ));

            return result;
        }

        /// <summary>
        /// Returns a clone of the image that has been rotated clockwise.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public ImageWrapper RotateImage(float degrees)
        {
            var clone = Safe_Native.Clone(context => context.Rotate(degrees));
            return new(clone, isCachedOriginal: false);
        }

        public ImageWrapper AddTextLines(IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            Image<Rgba32> clone;

            var markerTextFontFamilyName = FontCache.MarkerTextFontFamilyName;
            if(markerTextFontFamilyName == null) {
                clone = Safe_Native.Clone();
            } else {
                clone = Safe_Native.Clone(context => {
                    var lines =          textLines.Where(tl => tl != null).ToList();
                    var lineHeight =     isHighDpi ? 24f : 12f;
                    var topOffset =      5f;
                    var startPointSize = isHighDpi ? 20f : 10f;
                    var outlinePen =     isHighDpi ? PenCache.MarkerTextOutlinePenHiDpi : PenCache.MarkerTextOutlinePen;
                    var left =           centreText
                                            ? ((float)Width / 2f)
                                            : outlinePen.StrokeWidth / 2f;
                    var top =            (Height - topOffset) - (lines.Count * lineHeight);
                    var width =          Math.Max(0F, Width - outlinePen.StrokeWidth);
                    var fillOffset =     outlinePen.StrokeWidth / 2f;

                    var lineTop = top;
                    foreach(var line in lines) {
                        var fontAndText = FontCache.GetFontForText(
                            markerTextFontFamilyName,
                            FontCache.MarkerTextFontStyle,
                            startPointSize,
                            6f,
                            width,
                            lineHeight * 2f,
                            line,
                            useCache: true
                        );

                        var textOptions = new RichTextOptions(fontAndText.Font) {
                            Origin = new PointF(left + fillOffset, lineTop + fillOffset),
                            HorizontalAlignment = HorizontalAlignment.Center,
                        };

                        context.DrawText(
                            textOptions,
                            fontAndText.Text,
                            BrushCache.MarkerTextOutlineBrush,
                            outlinePen
                        );

                        textOptions.Origin = new PointF(left, lineTop);

                        context.DrawText(
                            textOptions,
                            fontAndText.Text,
                            BrushCache.MarkerTextFillBrush
                        );

                        lineTop += lineHeight;
                    }
                });
            }

            var result = new ImageWrapper(clone, isCachedOriginal: false);
            return result;
        }
    }
}
