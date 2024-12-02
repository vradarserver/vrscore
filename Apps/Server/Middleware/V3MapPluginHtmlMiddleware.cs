// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using Microsoft.AspNetCore.Http;
using VirtualRadar.Configuration;

namespace VirtualRadar.Server.Middleware
{
    public class V3MapPluginHtmlMiddleware
    {
        private readonly RequestDelegate _Next;
        private readonly ISettings<AircraftMapSettings> _AircraftMapSettings;

        public V3MapPluginHtmlMiddleware(
            RequestDelegate next,
            ISettings<AircraftMapSettings> aircraftMapSettings
        )
        {
            _Next = next;
            _AircraftMapSettings = aircraftMapSettings;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if(!IsMapHtmlPage(context)) {
                await _Next(context);
            } else {
                var originalResponseStream = context.Response.Body;
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                await _Next(context);

                if(!context.Response.ContentType?.Contains("text/html") == true) {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(originalResponseStream);
                    context.Response.Body = originalResponseStream;
                } else {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(memoryStream);
                    var content = await reader.ReadToEndAsync();

                    content = ReplaceContent(content);

                    // Write modified content back to the response
                    var modifiedContent = Encoding.UTF8.GetBytes(content);
                    context.Response.Body = originalResponseStream;
                    context.Response.ContentLength = modifiedContent.Length;
                    await context.Response.Body.WriteAsync(modifiedContent);
                }
            }
        }

        private static bool IsMapHtmlPage(HttpContext context)
        {
            var path = context.Request.Path;
            return path.StartsWithSegments("/v3/desktop.html", StringComparison.InvariantCultureIgnoreCase)
                ;
        }

        private string ReplaceContent(string content)
        {
            content ??= "";
            string mapStylesheet = null;
            string mapJavascript = null;

            switch(_AircraftMapSettings.LatestValue.MapProvider) {
                case MapProvider.GoogleMaps:
                    mapJavascript = @"<script src=""script/jquiplugin/jquery.vrs.map-google.js"" type=""text/javascript""></script>";
                    break;
                case MapProvider.Leaflet:
                    mapStylesheet = @"<link rel=""stylesheet"" href=""css/leaflet/leaflet.css"" type=""text/css"" media=""screen"" />
                                      <link rel=""stylesheet"" href=""css/leaflet.markercluster/MarkerCluster.css"" type=""text/css"" media=""screen"" />
                                      <link rel=""stylesheet"" href=""css/leaflet.markercluster/MarkerCluster.Default.css"" type=""text/css"" media=""screen"" />";
                    mapJavascript = @"<script src=""script/leaflet-src.js"" type=""text/javascript""></script>
                                      <script src=""script/leaflet.markercluster-src.js"" type=""text/javascript""></script>
                                      <script src=""script/jquiplugin/jquery.vrs.map-leaflet.js"" type=""text/javascript""></script>";
                    break;
                default:
                    throw new NotImplementedException();
            }

            if(!String.IsNullOrEmpty(mapStylesheet)) {
                content = content.Replace("<!-- [[ MAP STYLESHEET ]] -->", mapStylesheet);
            }
            if(!String.IsNullOrEmpty(mapJavascript)) {
                content = content.Replace("<!-- [[ MAP PLUGIN ]] -->", mapJavascript);
            }

            return content;
        }
    }
}
