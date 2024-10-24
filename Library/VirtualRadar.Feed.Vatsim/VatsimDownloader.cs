// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Timers;
using Newtonsoft.Json;
using VirtualRadar.Configuration;
using VirtualRadar.Feed.Vatsim.ApiModels;

namespace VirtualRadar.Feed.Vatsim
{
    /// <summary>
    /// The default implementation of <see cref="IVatsimDownloader"/>.
    /// </summary>
    /// <param name="_Settings"></param>
    /// <param name="_Log"></param>
    /// <param name="_HttpClient"></param>
    class VatsimDownloader(
        #pragma warning disable IDE1006 // VS2022 doesn't let you set naming rules for class primary ctors
        ISettings<VatsimSettings> _Settings,
        ILog _Log,
        IHttpClientService _HttpClient
        #pragma warning restore IDE1006
    ) : IVatsimDownloader
    {
        private CallbackList<VatsimDataV3> _DataDownloadedCallbacks = new();
        private object _SyncLock = new();       // <-- CallbackLists are already threadsafe, this is not used to gate access to those
        private System.Timers.Timer _Timer;     // <-- null if the timer has never been started or if it has been disposed
        private Status _Status;                 // <-- VATSIM status data, holds list of round-robin URLs to fetch from
        private DateTime _StatusDownloadedUtc;  // <-- time of last download of status, used to control when it'll be fetched again

        ~VatsimDownloader() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock(_SyncLock) {
                if(_Timer != null) {
                    _Timer.Enabled = false;
                    _Timer.Dispose();
                    _Timer = null;
                }
            }
        }

        /// <inheritdoc/>
        public ICallbackHandle AddDataDownloadedCallback(Action<VatsimDataV3> callback)
        {
            StartTimer();
            return _DataDownloadedCallbacks.Add(callback);
        }

        private void StartTimer()
        {
            if(_Timer == null) {
                lock(_SyncLock) {
                    if(_Timer == null) {
                        _Timer = new() {
                            AutoReset = false,
                            Interval =  1000,   // <-- first fetch is always 1 second from startup, after that we use the currently configured refresh interval
                            Enabled =   false,
                        };
                        _Timer.Elapsed += Timer_Elapsed;
                        _Timer.Start();
                    }
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(_DataDownloadedCallbacks.Count > 0) {
                try {
                    Task.Run(() => DownloadFromVatsim()).Wait();
                } catch(Exception ex) {
                    _Log.Exception(ex, "Exception encountered downloading from VATSIM");
                }
            }
            var interval = _Settings.LatestValue.RefreshIntervalSeconds * 1000;
            lock(_SyncLock) {
                if(_Timer != null) {
                    _Timer.Interval = interval;
                    _Timer.Enabled = true;
                }
            }
        }

        private async Task DownloadFromVatsim()
        {
            await DownloadStatus();
            await DownloadDataV3();
        }

        private async Task DownloadStatus()
        {
            if(_Status == null || _StatusDownloadedUtc.AddHours(_Settings.LatestValue.RefreshStatusHours) <= DateTime.UtcNow) {
                var jsonText = await _HttpClient.Shared.GetStringAsync(_Settings.LatestValue.StatusUrl);
                if(!String.IsNullOrEmpty(jsonText)) {
                    var status = JsonConvert.DeserializeObject<Status>(jsonText);
                    if((status.data?.v3.Count ?? 0) > 0) {
                        lock(_SyncLock) {
                            _Status = status;
                            _StatusDownloadedUtc = DateTime.UtcNow;
                        }
                    }
                }
            }
        }

        private async Task DownloadDataV3()
        {
            var status = _Status;
            if(status != null) {
                var url = RoundRobin.ChooseAtRandom(status.data.v3);
                if(!String.IsNullOrEmpty(url)) {
                    var jsonText = await _HttpClient.Shared.GetStringAsync(url);
                    if(!String.IsNullOrEmpty(jsonText)) {
                        var dataV3 = JsonConvert.DeserializeObject<VatsimDataV3>(jsonText);
                        _DataDownloadedCallbacks.InvokeWithoutExceptions(dataV3);
                    }
                }
            }
        }
    }
}
