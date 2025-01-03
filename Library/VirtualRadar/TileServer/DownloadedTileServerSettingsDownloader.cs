﻿// Copyright © 2018 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Diagnostics;
using Newtonsoft.Json;
using VirtualRadar.Configuration;

namespace VirtualRadar.TileServer
{
    /// <summary>
    /// Default implementation of <see cref="IDownloadedTileServerSettingsDownloader"/>
    /// </summary>
    class DownloadedTileServerSettingsDownloader(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        ISettings<TileServerSettings> _TileServerSettings,
        IHttpClientService _HttpClient,
        ILog _Log
        #pragma warning restore IDE1006
    ) : IDownloadedTileServerSettingsDownloader
    {
        /// <inheritdoc/>
        public async Task<IReadOnlyList<DownloadedTileServerSettings>> DownloadAsync(CancellationToken cancellationToken)
        {
            var url = _TileServerSettings.LatestValue.DownloadUrl;
            _Log.Message($"Attempting to download tile server settings from {url}");
            var stopwatch = Stopwatch.StartNew();
            var jsonText = await _HttpClient.Shared.GetStringAsync(
                url,
                cancellationToken
            );
            stopwatch.Stop();
            _Log.Message(
                $"Downloaded {jsonText?.Length.ToString("N0") ?? "0"} characters of tile server settings in {stopwatch.ElapsedMilliseconds:N0} ms" +
                $"{(cancellationToken.IsCancellationRequested ? " (request cancelled)" : "")}"
            );

            DownloadedTileServerSettings[] result = null;
            if(!cancellationToken.IsCancellationRequested && !String.IsNullOrEmpty(jsonText)) {
                result = JsonConvert.DeserializeObject<DownloadedTileServerSettings[]>(jsonText);
            }

            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyList<DownloadedTileServerSettings> Download(int timeoutSeconds = 30)
        {
            using(var cancellationSource = timeoutSeconds > 0
                ? new CancellationTokenSource(timeoutSeconds * 1000)
                : new CancellationTokenSource()
            ) {
                return Task.Run(() => DownloadAsync(
                    cancellationSource.Token
                )).Result;
            }
        }
    }
}
