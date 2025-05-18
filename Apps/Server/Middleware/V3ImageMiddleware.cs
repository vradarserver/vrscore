// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.AspNetCore.Http;
using VirtualRadar.Configuration;
using VirtualRadar.Drawing;
using ImageResources = VirtualRadar.Resources.Images;
using VirtualRadar.WebSite;
using VirtualRadar.WebSite.Models;
using Microsoft.AspNetCore.Hosting;

namespace VirtualRadar.Server.Middleware
{
    /// <summary>
    /// Handles requests for images for the V3 site.
    /// </summary>
    public class V3ImageMiddleware(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        RequestDelegate                         _NextMiddleware,
        IWebHostEnvironment                     _WebHostEnvironment,
        GetImageModelBuilder                    _RequestBuilder,
        IGraphics                               _Graphics,
        IFileSystem                             _FileSystem,
        ISettings<OperatorAndTypeFlagSettings>  _OperatorFlagSettings,
        ISettings<InternetClientSettings>       _InternetClientSettings
        #pragma warning restore IDE1006
    )
    {
        const string _PathPrefix = "/v3/";
        const string _ImagesPrefix = _PathPrefix + "images";

        public async Task InvokeAsync(HttpContext context)
        {
            if(!context.Request.Path.StartsWithSegments(_ImagesPrefix, StringComparison.InvariantCultureIgnoreCase)) {
                await _NextMiddleware(context);
            } else {
                var requestHandled = await ProcessRequest(context);
                if(!requestHandled) {
                    await _NextMiddleware(context);
                }
            }
        }

        private async Task<bool> ProcessRequest(HttpContext context)
        {
            var imageRequest = _RequestBuilder.ExtractImageRequestFromWebPath(context.Request.Path.Value);
            var result = imageRequest != null;

            if(result) {
                result = false;

                if(imageRequest.WebSiteFileName != null) {
                    result = await ServeImageFromFile(context, imageRequest);
                }
                if(!result) {
                    switch(imageRequest.ImageName) {
                        case "AIRPLANE":            result = await ServeResourceImage(context, ImageResources.Marker_Airplane, imageRequest); break;
                        case "AIRPLANESELECTED":    result = await ServeResourceImage(context, ImageResources.Marker_AirplaneSelected, imageRequest); break;
                        case "COMPASS":             result = await ServeResourceImage(context, ImageResources.Compass, imageRequest); break;
                        case "OPFLAG":              result = await ServeFlag(context, imageRequest, isTypeFlag: false); break;
                        case "TYPE":                result = await ServeFlag(context, imageRequest, isTypeFlag: true); break;
                        case "YOUAREHERE":          result = await ServeResourceImage(context, ImageResources.BlueBall, imageRequest); break;
                    }
                }
            }

            return result;
        }

        private static readonly char[] _PipeCharacterArray = [ '|' ];

        private async Task<bool> ServeFlag(HttpContext context, GetImageModel imageRequest, bool isTypeFlag)
        {
            var settings = _OperatorFlagSettings.LatestValue;
            var folder = isTypeFlag
                ? settings.TypeFlagsFolder
                : settings.OperatorFlagsFolder;

            IImage image = null;
            if(!String.IsNullOrEmpty(folder)) {
                var chunks = imageRequest
                    .File
                    .Split(_PipeCharacterArray, StringSplitOptions.RemoveEmptyEntries);

                foreach(var chunk in chunks) {
                    if(_FileSystem.IsValidFileName(chunk)) {
                        var fullPath = _FileSystem.Combine(folder, $"{chunk}.bmp");
                        if(_FileSystem.FileExists(fullPath)) {
                            var imageBytes = await _FileSystem.ReadAllBytesAsync(fullPath);
                            image = _Graphics.CreateImage(imageBytes);
                            break;
                        }
                    }
                }
            }
            image ??= _Graphics.CreateBlankImage(imageRequest.Width ?? 1, imageRequest.Height ?? 1);

            try {
                return await SendImage(context, image, imageRequest.ImageFormat);
            } finally {
                _Graphics.DisposeIfNotCachedOriginal(image);
            }
        }

        private async Task<bool> ServeImageFromFile(HttpContext context, GetImageModel imageRequest)
        {
            var result = false;

            var fileName = $"{_PathPrefix}{imageRequest.WebSiteFileName}";
            var fileProvider = _WebHostEnvironment.WebRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(fileName);
            if(fileInfo.Exists) {
                var bytes = await _FileSystem.ReadAllBytesAsync(fileInfo.PhysicalPath);
                var image = _Graphics.CreateImage(bytes);
                try {
                    image = ApplyTransformations(context, image, imageRequest);
                    result = await SendImage(context, image, imageRequest.ImageFormat);
                } finally {
                    _Graphics.DisposeIfNotCachedOriginal(image);
                }
            }

            return result;
        }

        private async Task<bool> ServeResourceImage(HttpContext context, byte[] imageBytes, GetImageModel imageRequest)
        {
            var image = _Graphics.CreateImage(imageBytes);

            try {
                image = ApplyTransformations(context, image, imageRequest);
                return await SendImage(context, image, imageRequest.ImageFormat);
            } finally {
                _Graphics.DisposeIfNotCachedOriginal(image);
            }
        }

        private IImage ApplyTransformations(HttpContext context, IImage image, GetImageModel imageRequest)
        {
            if(imageRequest.IsHighDpi) {
                image = _Graphics.ResizeForHiDpi(image);
            }
            if((imageRequest.RotateDegrees ?? 0) != 0) {
                image = _Graphics.RotateImage(image, imageRequest.RotateDegrees.Value);
            }
            if(imageRequest.Width != null) {
                image = _Graphics.WidenImage(image, imageRequest.Width.Value, imageRequest.CentreImageHorizontally);
            }
            if(imageRequest.ShowAltitudeStalk) {
                image = _Graphics.AddAltitudeStalk(image, imageRequest.Height ?? 1, imageRequest.CentreX ?? 1);
            } else if(imageRequest.Height != null) {
                image = _Graphics.HeightenImage(image, imageRequest.Height.Value, imageRequest.CentreImageVertically);
            }

            if(imageRequest.HasTextLines) {
                var isInternet = context.Items.ContainsKey(HttpContextItemKey.VrsIsInternet);
                var isAllowed = !isInternet || _InternetClientSettings.LatestValue.CanShowPinText;
                if(isAllowed) {
                    image = _Graphics.AddTextLines(image, imageRequest.TextLines, centreText: true, isHighDpi: imageRequest.IsHighDpi);
                }
            }

            return image;
        }

        private async Task<bool> SendImage(HttpContext context, IImage image, ImageFormat format)
        {
            var result = false;

            var imageBytes = image == null ? [] : _Graphics.GetImageBytes(image, format);
            if(imageBytes?.Length > 0) {
                result = true;
                context.Response.ContentType = format.ToMimeType();
                await context.Response.Body.WriteAsync(imageBytes);
            }

            return result;
        }
    }
}
