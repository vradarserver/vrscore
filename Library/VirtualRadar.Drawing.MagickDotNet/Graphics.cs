﻿// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Drawing.MagickDotNet
{
    class Graphics : IGraphics
    {
        public IImage AddAltitudeStalk(IImage original, int height, int centreX)
        {
            throw new NotImplementedException();
        }

        public IImage AddTextLines(IImage image, IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            throw new NotImplementedException();
        }

        public IImage CreateBlankImage(int width, int height) => new Image(width, height);

        public IImage CreateIPhoneSplash(string webSiteAddress, bool isIPad, List<string> pathParts)
        {
            throw new NotImplementedException();
        }

        public IImage HeightenImage(IImage original, int height, bool centreVertically)
        {
            throw new NotImplementedException();
        }

        public IImage ResizeBitmap(IImage original, int width, int height, ResizeMode mode, IBrush zoomBackground, bool preferSpeedOverQuality)
        {
            throw new NotImplementedException();
        }

        public IImage ResizeForHiDpi(IImage original)
        {
            throw new NotImplementedException();
        }

        public IImage RotateImage(IImage original, double degrees)
        {
            throw new NotImplementedException();
        }

        public IImage UseImage(IImage tempImage, IImage newImage)
        {
            throw new NotImplementedException();
        }

        public IImage WidenImage(IImage original, int width, bool centreHorizontally)
        {
            throw new NotImplementedException();
        }
    }
}
