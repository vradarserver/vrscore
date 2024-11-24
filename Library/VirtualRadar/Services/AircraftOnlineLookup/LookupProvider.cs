// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Newtonsoft.Json;
using VirtualRadar.Configuration;
using VirtualRadar.Message;

namespace VirtualRadar.Services.AircraftOnlineLookup
{
    class LookupProvider : IAircraftOnlineLookupProvider
    {
        private readonly object _SyncLock = new();
        private readonly IHttpClientService _HttpClient;
        private readonly ISettings<AircraftOnlineLookupServiceSettings> _LookupSettings;
        private ServerSettings _ServerSettings;
        private DateTime _ServerSettingsFetchedUtcNow;

        /// <inheritdoc/>
        public int MaxBatchSize => _ServerSettings?.MaxBatchSize ?? 10;

        /// <inheritdoc/>
        public int MinSecondsBetweenRequests => _ServerSettings?.MinSeconds ?? 1;

        /// <inheritdoc/>
        public int MaxSecondsAfterFailedRequest => _ServerSettings?.MaxSeconds ?? 30;

        /// <inheritdoc/>
        public string DataSupplier => _ServerSettings?.DataSupplier;

        /// <inheritdoc/>
        public string SupplierCredits => _ServerSettings?.SupplierCredits;

        /// <inheritdoc/>
        public string SupplierWebSiteUrl => _ServerSettings?.SupplierUrl;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webAddresses"></param>
        /// <param name="httpClientService"></param>
        public LookupProvider(
            IWebAddressManager webAddresses,
            IHttpClientService httpClientService,
            ISettings<AircraftOnlineLookupServiceSettings> lookupSettings
        )
        {
            _HttpClient = httpClientService;
            _LookupSettings = lookupSettings;
        }

        /// <inheritdoc/>
        public async Task InitialiseSupplierDetails(CancellationToken cancellationToken)
        {
            await FetchSettings(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<BatchedLookupOutcome<LookupByIcaoOutcome>> LookupIcaos(
            IEnumerable<Icao24> icaos,
            CancellationToken cancellationToken
        )
        {
            var result = new BatchedLookupOutcome<LookupByIcaoOutcome>();

            await FetchSettings(cancellationToken);

            var lookupUrl = _ServerSettings?.LookupByIcaoUrl;
            if(!String.IsNullOrEmpty(lookupUrl)) {
                var joinedIcaos = String.Join(
                    "-",
                    icaos.Select(icao => icao.ToString())
                );
                var contentBody = new Dictionary<string, string> {
                    { "icaos", joinedIcaos },
                };

                using var content = new FormUrlEncodedContent(contentBody);
                using var request = new HttpRequestMessage(HttpMethod.Post, lookupUrl) {
                    Content = content,
                };

                using var response = await _HttpClient.Shared.SendAsync(request, cancellationToken);

                if(response.IsSuccessStatusCode && !cancellationToken.IsCancellationRequested) {
                    var jsonText = await response.Content.ReadAsStringAsync(cancellationToken);
                    if(!String.IsNullOrEmpty(jsonText) && !cancellationToken.IsCancellationRequested) {
                        var sourceAge = DateTime.UtcNow;
                        var seenIcaos = new HashSet<Icao24>();

                        var allAircraft = JsonConvert.DeserializeObject<StandingDataSiteAircraft[]>(jsonText);
                        foreach(var sdmAircraft in allAircraft) {
                            if(seenIcaos.Add(sdmAircraft.Icao24)) {
                                result.Found.Add(new(sdmAircraft.Icao24, success: true) {
                                    Country =           sdmAircraft.Country,
                                    Manufacturer =      sdmAircraft.Manufacturer,
                                    Model =             sdmAircraft.Model,
                                    ModelIcao =         sdmAircraft.ModelIcao,
                                    Operator =          sdmAircraft.Operator,
                                    OperatorIcao =      sdmAircraft.OperatorIcao,
                                    Registration =      sdmAircraft.Registration,
                                    Serial =            sdmAircraft.Serial,
                                    SourceAgeUtc =      sourceAge,
                                    YearBuilt =   sdmAircraft.YearBuilt,
                                });
                            }
                        }

                        foreach(var icao in icaos) {
                            if(seenIcaos.Add(icao)) {
                                result.Missing.Add(new(icao, success: false));
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches the settings from the server. These are fetched once on startup and then once every hour.
        /// </summary>
        /// <param name="cancellationToken"></param>
        private async Task FetchSettings(CancellationToken cancellationToken)
        {
            if(ServerSettingsNeedRefetch()) {
                var lookupSettings = _LookupSettings.LatestValue;
                var url = lookupSettings.LookupUrl
                    .Replace("{language}", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await _HttpClient.Shared.SendAsync(request, cancellationToken);

                if(response.IsSuccessStatusCode) {
                    var jsonText = await response.Content.ReadAsStringAsync(cancellationToken);
                    if(!String.IsNullOrEmpty(jsonText) && !cancellationToken.IsCancellationRequested) {
                        lock(_SyncLock) {
                            if(ServerSettingsNeedRefetch()) {
                                _ServerSettings = JsonConvert.DeserializeObject<ServerSettings>(jsonText);
                                _ServerSettingsFetchedUtcNow = DateTime.UtcNow;
                            }
                        }
                    }
                }
            }
        }

        private bool ServerSettingsNeedRefetch() => _ServerSettings == null || _ServerSettingsFetchedUtcNow.AddHours(1) <= DateTime.UtcNow;
    }
}
