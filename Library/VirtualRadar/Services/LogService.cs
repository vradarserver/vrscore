// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Extensions.Options;
using VirtualRadar.Configuration;
using VirtualRadar.Extensions;

namespace VirtualRadar.Services
{
    /// <inheritdoc/>
    class LogService(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        IWorkingFolder _WorkingFolder,
        IFileSystem _FileSystem,
        IOptions<LogOptions> _Settings
        #pragma warning restore IDE1006
    ) : ILog
    {
        public const string FileName = "VirtualRadarCoreLog.txt";

        public string FullPath => _FileSystem.Combine(_WorkingFolder.Folder, FileName);

        /// <inheritdoc/>
        public void Message(string message)
        {
            // We hide any failures to write to the log because we can't log them, and they could
            // mask the underlying error that's causing the failure in the first place
            try {
                if(!String.IsNullOrWhiteSpace(message)) {
                    var content = LoadContent();
                    var timestamp = $"[{DateTime.UtcNow:R}]";
                    message = $"{timestamp} {message}";
                    foreach(var line in message.SplitIntoLines()) {
                        content.Add(line);
                    }
                    TrimContent(content);
                    SaveContent(content);
                }
            } catch {
                ;
            }
        }

        /// <inheritdoc/>
        public void Exception(Exception ex, string message = null)
        {
            // We hide any failures to write to the log because we can't log them, and they could
            // mask the underlying error that's causing the failure in the first place
            try {
                Message(message ?? "Exception caught during processing");
                Message(ex.ToString());
            } catch {
                ;
            }
        }

        private List<string> LoadContent()
        {
            var result = new List<string>();

            if(_FileSystem.FileExists(FullPath)) {
                result.AddRange(_FileSystem.ReadAllLines(FullPath));
            }

            return result;
        }

        private void SaveContent(List<string> content)
        {
            _FileSystem.WriteAllLines(FullPath, content);
        }

        private void TrimContent(List<string> lines)
        {
            var maximumLines = Math.Max(0, _Settings.Value.MaximumLines);
            while(lines.Count > maximumLines) {
                lines.RemoveAt(0);
            }
        }
    }
}
