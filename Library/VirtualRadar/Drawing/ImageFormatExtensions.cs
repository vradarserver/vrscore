﻿// Copyright © 2024 onwards, Andrew Whewell
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
    public static class ImageFormatExtensions
    {
        public static ImageFormat? FromExtension(string extension)
        {
            switch(extension?.ToUpperInvariant()) {
                case ".BMP":    return ImageFormat.Bmp;
                case ".GIF":    return ImageFormat.Gif;
                case ".JPG":    return ImageFormat.Jpeg;
                case ".JPEG":   return ImageFormat.Jpeg;
                case ".PNG":    return ImageFormat.Png;
                default:        return null;
            }
        }

        public static string ToMimeType(this ImageFormat imageFormat)
        {
            switch(imageFormat) {
                case ImageFormat.Bmp:   return MimeType.BitmapImage;
                case ImageFormat.Gif:   return MimeType.GifImage;
                case ImageFormat.Jpeg:  return MimeType.JpegImage;
                case ImageFormat.Png:   return MimeType.PngImage;
                default:                return "";
            }
        }

        public static bool IsTransparencySupported(this ImageFormat imageFormat)
        {
            return imageFormat == ImageFormat.Png
                || imageFormat == ImageFormat.Gif;
        }
    }
}
