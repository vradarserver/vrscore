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
using VirtualRadar.WebSite;
using VirtualRadar.WebSite.Models;

namespace VirtualRadar.Server.Middleware
{
    /// <summary>
    /// Handles requests for images for the V3 site.
    /// </summary>
    public class V3ImageMiddleware(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        RequestDelegate                         _NextMiddleware,
        GetImageModelBuilder                    _RequestBuilder,
        IGraphics                               _Graphics,
        IFileSystem                             _FileSystem,
        ISettings<OperatorAndTypeFlagSettings>  _OperatorFlagSettings
        #pragma warning restore IDE1006
    )
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if(!context.Request.Path.StartsWithSegments("/v3/images", StringComparison.InvariantCultureIgnoreCase)) {
                await _NextMiddleware(context);
            } else {
                var requestHandled = await ProcessRequest(context);
                if(!requestHandled) {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                }
            }
        }

        private async Task<bool> ProcessRequest(HttpContext context)
        {
            var imageRequest = _RequestBuilder.ExtractImageRequestFromWebPath(context.Request.Path.Value);
            var result = imageRequest != null;

            if(result) {
                result = false;
                switch(imageRequest.ImageName) {
                    case "OPFLAG":  result = await ServeFlag(context, imageRequest, isTypeFlag: false); break;
                    case "TYPE":    result = await ServeFlag(context, imageRequest, isTypeFlag: true); break;
                }
            }

            return result;
        }

        private static readonly char[] _PipeCharacterArray = [ '|' ];

        private async Task<bool> ServeFlag(HttpContext context, GetImageModel imageRequest, bool isTypeFlag)
        {
            var result = false;
            byte[] imageBytes = null;

            var settings = _OperatorFlagSettings.LatestValue;
            var folder = isTypeFlag
                ? settings.TypeFlagsFolder
                : settings.OperatorFlagsFolder;

            if(!String.IsNullOrEmpty(folder)) {
                var chunks = imageRequest
                    .File
                    .Split(_PipeCharacterArray, StringSplitOptions.RemoveEmptyEntries);

                foreach(var chunk in chunks) {
                    if(_FileSystem.IsValidFileName(chunk)) {
                        var fullPath = _FileSystem.Combine(folder, $"{chunk}.bmp");
                        if(_FileSystem.FileExists(fullPath)) {
                            result = true;
                            imageBytes = await _FileSystem.ReadAllBytesAsync(fullPath);
                            break;
                        }
                    }
                }
            }

            if(imageBytes == null) {
                imageBytes = _Graphics
                    .CreateBlankImage(settings.FlagWidthPixels, settings.FlagHeightPixels)
                    .GetImageBytes(imageRequest.ImageFormat);
            }

            if(imageBytes != null) {
                result = true;
                context.Response.ContentType = imageRequest.ImageFormat.ToMimeType();
                await context.Response.Body.WriteAsync(imageBytes);
            }

            return result;
        }
    }
}
