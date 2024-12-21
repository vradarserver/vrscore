// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Drawing;
using VirtualRadar.WebSite.Models;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Extracts <see cref="GetImageModel"/> request models from various sources.
    /// </summary>
    [Lifetime(Lifetime.Transient)]
    public class GetImageModelBuilder(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        IFileSystem _FileSystem
        #pragma warning restore IDE1006
    )
    {
        /// <summary>
        /// Extracts the image request from a web site request path.
        /// </summary>
        /// <param name="requestPath"></param>
        /// <returns></returns>
        public GetImageModel ExtractImageRequestFromWebPath(string requestPath)
        {
            var fileName = _FileSystem.GetFileName(requestPath);
            var imageFormat = ImageFormatExtensions.FromExtension(
                _FileSystem.GetExtension(fileName)
            );

            var result = imageFormat == null
                ? null
                : new GetImageModel() {
                    ImageName = _FileSystem.GetFileNameWithoutExtension(fileName).ToUpperInvariant(),
                };
            if(result != null) {
                ExtractImageRequestFromPathSegments(requestPath, fileName, result);
            }

            return result;
        }

        private void ExtractImageRequestFromPathSegments(string requestPath, string requestFileName, GetImageModel result)
        {
            foreach(var pathPart in requestPath.Split('/')) {
                var caselessPart = pathPart.ToUpperInvariant();

                if(caselessPart.StartsWith("ALT-")) {
                    result.ShowAltitudeStalk = true;
                }

                if(caselessPart.StartsWith("ROTATE-")) {
                    result.RotateDegrees = ParseDouble(pathPart[7..], 0.0, 359.99);
                } else if(caselessPart.StartsWith("HGHT-")) {
                    result.Height = ParseInt(pathPart[5..], 0, 4096);
                } else if(caselessPart.StartsWith("WDTH-")) {
                    result.Width = ParseInt(pathPart[5..], 0, 4096);
                } else if(caselessPart.StartsWith("CENX-")) {
                    result.CentreX = ParseInt(pathPart[5..], 0, 4096);
                } else if(caselessPart.StartsWith("FILE-")) {
                    result.File = pathPart[5..].Replace("\\", "");
                } else if(caselessPart.StartsWith("SIZE-")) {
                    result.Size = StandardWebSiteImageSizeExtensions.ParseStandardSize(pathPart[5..]);
                } else if(caselessPart == "HIDPI") {
                    result.IsHighDpi = true;
                } else if(caselessPart == "LEFT") {
                    result.CentreImageHorizontally = false;
                } else if(caselessPart == "TOP") {
                    result.CentreImageVertically = false;
                } else if(caselessPart == "NO-CACHE") {
                    result.NoCache = false; // TODO: !context.IsInternet;
                } else if(caselessPart.StartsWith("WEB")) {
                    var pathAndFileName = new StringBuilder("/images/");
                    var hyphenPosn = pathPart.IndexOf('-');
                    if(hyphenPosn != -1) {
                        var folder = pathPart.Substring(hyphenPosn + 1).Replace("\\", "/").Trim();
                        if(folder.Length > 0) {
                            pathAndFileName.AppendFormat("{0}{1}", folder, folder[^1] == '/' ? "" : "/");
                        }
                    }
                    pathAndFileName.Append(_FileSystem.GetFileName(requestFileName));
                    result.WebSiteFileName = pathAndFileName.ToString();
                } else if(caselessPart.StartsWith("PL")) {
                    var hyphenPosn = caselessPart.IndexOf('-');
                    if(hyphenPosn >= 2) {
                        var rowText = caselessPart[2..hyphenPosn ];
                        if(int.TryParse(rowText, out var row) && row >= 1 && row <= 9) {
                            --row;
                            while(result.TextLines.Count <= row) {
                                result.TextLines.Add(null);
                            }
                            result.TextLines[row] = pathPart[(hyphenPosn + 1)..];
                        }
                    }
                }
            }
        }

        private static double? ParseDouble(string value, double min = -4096.0, double max = 4096.0)
        {
            return double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
                ? Math.Min(max, Math.Max(min, result))
                : (double?)null;
        }

        private static int? ParseInt(string value, int min = -4096, int max = 4096)
        {
            return int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
                ? Math.Min(max, Math.Max(min, result))
                : (int?)null;
        }
    }
}
