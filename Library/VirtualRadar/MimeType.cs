﻿// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Extensions;

namespace VirtualRadar
{
    /// <summary>
    /// A collection of different mime types.
    /// </summary>
    public static class MimeType
    {
        /// <summary>
        /// A map of all extensions to mime types.
        /// </summary>
        private static Dictionary<string, string> _ExtensionToMimeType = new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets the MIME type for a .BMP file.
        /// </summary>
        public static string BitmapImage => GetForExtension("bmp");

        /// <summary>
        /// Gets the MIME type for a cascading stylesheet file.
        /// </summary>
        public static string Css => GetForExtension("css");

        /// <summary>
        /// Gets the MIME type for a URL encoded form.
        /// </summary>
        public static string FormUrlEncoded => "application/x-www-form-urlencoded";

        /// <summary>
        /// Gets the MIME type for a .GIF file.
        /// </summary>
        public static string GifImage => GetForExtension("gif");

        /// <summary>
        /// Gets the MIME type for an HTML file.
        /// </summary>
        public static string Html => GetForExtension("html");

        /// <summary>
        /// Gets the MIME type for a .ICO file.
        /// </summary>
        public static string IconImage => GetForExtension("ico");

        /// <summary>
        /// Gets the MIME type for a .JS file.
        /// </summary>
        public static string Javascript => GetForExtension("js");

        /// <summary>
        /// Gets the MIME type for a .JPG file.
        /// </summary>
        public static string JpegImage => GetForExtension("jpeg");

        /// <summary>
        /// Gets the MIME type for a JSON text file.
        /// </summary>
        public static string Json => GetForExtension("json");

        /// <summary>
        /// Gets the MIME type for a .PNG file.
        /// </summary>
        public static string PngImage => GetForExtension("png");

        /// <summary>
        /// Gets the MIME type for a plain text file.
        /// </summary>
        public static string Text => GetForExtension("txt");

        /// <summary>
        /// Gets the MIME type for a .TIF file.
        /// </summary>
        public static string TiffImage => GetForExtension("tiff");

        /// <summary>
        /// Gets the MIME type for a .WAV file.
        /// </summary>
        public static string WaveAudio => GetForExtension("wav");

        /// <summary>
        /// Returns the MIME type for a file extension. The leading full-stop on the extension is optional.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetForExtension(string extension)
        {
            var extensionMap = _ExtensionToMimeType;
            if(extensionMap.Count == 0) {
                extensionMap = LoadMimeTypes();
            }

            string result = null;
            if(!String.IsNullOrEmpty(extension)) {
                if(extension[0] == '.') {
                    extension = extension[1..];
                }
                if(!extensionMap.TryGetValue(extension, out result)) {
                    result = "application/octet-stream";
                }
            }

            return result;
        }

        /// <summary>
        /// Returns an array of all known file extensions.
        /// </summary>
        /// <returns></returns>
        public static string[] GetKnownExtensions()
        {
            var extensionMap = _ExtensionToMimeType;
            if(extensionMap.Count == 0) {
                extensionMap = LoadMimeTypes();
            }

            return extensionMap.Keys.ToArray();
        }

        /// <summary>
        /// Populates MimeTypes from resources.
        /// </summary>
        private static Dictionary<string, string> LoadMimeTypes()
        {
            var extensionMap = new Dictionary<string, string>();

            var mimeTypeResourceLines = ResxResources
                .MimeTypes
                .Split(
                    StringExtensions.AllLineEndingCharacters,
                    StringSplitOptions.RemoveEmptyEntries
                );

            foreach(var line in mimeTypeResourceLines) {
                if(line.Length > 0 && line[0] != '#') {
                    var chunks = line.Split(
                        StringExtensions.AllAsciiWhiteSpaceCharacters,
                        StringSplitOptions.RemoveEmptyEntries
                    );
                    if(chunks.Length > 1) {
                        var mimeType = chunks[0];
                        if(mimeType.Contains('/')) {
                            for(var i = 1;i < chunks.Length;++i) {
                                var extension = chunks[i];
                                extensionMap.TryAdd(extension, mimeType);
                            }
                        }
                    }
                }
            }
            _ExtensionToMimeType = extensionMap;

            return extensionMap;
        }
    }
}
