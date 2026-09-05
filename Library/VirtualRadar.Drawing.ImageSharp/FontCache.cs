// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using SixLabors.Fonts;

namespace VirtualRadar.Drawing.ImageSharp
{
    static class FontCache
    {
        record FontCacheKey(string FamilyName, FontStyle Style, float PointSize)
        {
        }

        private static readonly object _SyncLock = new();

        private static Dictionary<FontCacheKey, Font> _FontCache = [];

        public static FontStyle MarkerTextFontStyle { get; } = FontStyle.Bold;

        public static string? MarkerTextFontFamilyName { get; } = ResolveFontFamily(MarkerTextFontStyle,
            "Microsoft Sans Serif",
            "MS Reference Sans Serif",
            "Verdana",
            "Tahoma",
            "Roboto",
            "Droid Sans",
            "MS Sans Serif",
            "Helvetica",
            "Sans Serif",
            "Sans"
        );

        public static (Font Font, string Text) GetFontForText(
            string familyName,
            FontStyle style,
            float maxPointSize,
            float minPointSize,
            float constrainToWidth,
            float constrainToHeight,
            string text,
            bool useCache
        )
        {
            text ??= "";
            StringBuilder? buffer = null;
            var bufferContent = text;

            var pointSize = maxPointSize;
            bool trySmaller;
            Font font;
            TextOptions? textOptions = null;

            do {
                font = CreateFont(familyName, style, pointSize, useCache);
                if(textOptions?.Font != font) {
                    textOptions = new TextOptions(font);
                }
                var size = TextMeasurer.MeasureSize(bufferContent, textOptions);

                trySmaller =  size.Width > constrainToWidth
                           || size.Height > constrainToHeight;
                if(trySmaller) {
                    if(pointSize > minPointSize) {
                        pointSize = Math.Max(minPointSize, pointSize - 0.25F);
                    } else {
                        trySmaller = bufferContent.Length > 1;
                        if(trySmaller) {
                            buffer ??= new(bufferContent);
                            buffer.Remove(buffer.Length - 1, 1);
                            bufferContent = buffer.ToString();
                        }
                    }
                }
            } while(trySmaller);

            return (
                font,
                bufferContent
            );
        }

        public static string? ResolveFontFamily(FontStyle targetStyle, params string[] targetFontFamilies)
        {
            var installedFamilies = SystemFonts.Families.ToArray();

            string? result = null;
            foreach(var targetFontFamily in targetFontFamilies.Where(r => !String.IsNullOrWhiteSpace(r))) {
                foreach(var fontFamily in installedFamilies) {
                    if(String.Equals(fontFamily.Name, targetFontFamily, StringComparison.InvariantCultureIgnoreCase)) {
                        if(fontFamily.GetAvailableStyles().Contains(targetStyle)) {
                            result = fontFamily.Name;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private static Font CreateFont(
            string familyName,
            FontStyle style,
            float pointSize,
            bool useCache
        )
        {
            var key = new FontCacheKey(familyName, style, pointSize);
            lock(_SyncLock) {
                if(!useCache || !_FontCache.TryGetValue(key, out var result)) {
                    result = SystemFonts.CreateFont(familyName, pointSize, style);
                    if(useCache) {
                        _FontCache.Add(key, result);
                    }
                }

                return result;
            }
        }
    }
}
