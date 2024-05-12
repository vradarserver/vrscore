// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;
using Microsoft.Extensions.Options;
using VirtualRadar.Configuration;
using VirtualRadar.Extensions;

namespace VirtualRadar.Utility.CLIConsole
{
    class HeaderService
    {
        private ApplicationSettings _ApplicationSettings;

        public HeaderService(IOptions<ApplicationSettings> applicationSettings)
        {
            _ApplicationSettings = applicationSettings.Value;
        }

        /// <summary>
        /// Gets the copyright notice as a single line.
        /// </summary>
        /// <returns></returns>
        public string CopyrightNotice => $"{_ApplicationSettings.ApplicationName} v{_ApplicationSettings.VersionDescription} copyright (C) 2024 onwards, Andrew Whewell";

        /// <summary>
        /// Outputs the <see cref="CopyrightNotice"/> to the console.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public async Task OutputCopyright() => await Console.Out.WriteLineAsync(CopyrightNotice);

        /// <summary>
        /// Outputs the title and an underline.
        /// </summary>
        /// <param name="title"></param>
        public async Task OutputTitle(string title)
        {
            await Console.Out.WriteLineAsync(title);
            await Console.Out.WriteLineAsync(new String('=', title.Length));
        }

        /// <summary>
        /// Outputs each name/value pair on separate lines, with the values lines up underneath each other.
        /// </summary>
        /// <param name="nameValuePairs"></param>
        public async Task OutputOptions(params (string Name, string Value)[] nameValuePairs)
        {
            var longestName = nameValuePairs.DefaultIfEmpty().Max(r => r.Name?.Length ?? 0);
            var lineBuffer = new StringBuilder();

            foreach(var nameValuePair in nameValuePairs) {
                lineBuffer.Clear();

                if(!String.IsNullOrEmpty(nameValuePair.Name)) {
                    lineBuffer.Append(nameValuePair.Name);
                    lineBuffer.Append(':');
                }
                lineBuffer.AppendMany((longestName + 2) - lineBuffer.Length, ' ');
                if(!String.IsNullOrEmpty(nameValuePair.Value)) {
                    lineBuffer.Append(nameValuePair.Value);
                }

                await Console.Out.WriteLineAsync(lineBuffer.ToString());
            }
        }
    }
}
