// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Configuration;
using VirtualRadar.Extensions;

namespace VirtualRadar.StandingData
{
    /// <summary>
    /// The default implementation of <see cref="IRegistrationPrefixLookup"/>.
    /// </summary>
    /// <remarks>
    /// This fetches a CSV file from the VRS standing data repository to provide all of
    /// the prefixes. It maintains a local cache and refreshes the file roughly once a
    /// day, or at startup.
    /// </remarks>
    class RegistrationPrefixLookup(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        ISettings<RegistrationPrefixLookupSettings> _Settings,
        IWorkingFolder _WorkingFolder,
        IFileSystem _FileSystem,
        IHttpClientService _HttpClient,
        ILog _Log
        #pragma warning restore IDE1006
    ) : IRegistrationPrefixLookup
    {
        private bool _Loaded;

        // The parser that extracts values out of the CSV file. It's thread-safe.
        private readonly CsvParser _CsvParser = new();

        // Locks writes to maps etc. Always take a reference to the map when reading
        // and lock when overwriting.
        private readonly object _SyncLock = new();

        // A map of the first letter of a registration to a bucket of details for prefixes
        // that start with that letter. The letter is always upper-case, as are the prefixes.
        private volatile Dictionary<char, RegistrationPrefixDetail[]> _RegistrationFirstLetterToDetailsMap;

        // Times at UTC of the last download attempt (whether successful or not) and the last
        // successful download attempt.
        private DateTime _LastAttemptUtc;
        private DateTime _LastSuccessfulDownloadUtc;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fullRegistration"></param>
        /// <returns></returns>
        public RegistrationPrefixDetail FindDetailForFullRegistration(string fullRegistration)
        {
            Initialise();
            StartDownloadIfOutOfDate();

            var normalisedRegistration = NormaliseRegistration(fullRegistration, removeHyphen: false);

            return FindPrefixDetail(
                normalisedRegistration,
                prefix => prefix.DecodeFullRegex.IsMatch(normalisedRegistration)
            ).FirstOrDefault();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="noHyphenRegistration"></param>
        /// <returns></returns>
        public IList<RegistrationPrefixDetail> FindDetailsForNoHyphenRegistration(string noHyphenRegistration)
        {
            Initialise();
            StartDownloadIfOutOfDate();

            var normalisedRegistration = NormaliseRegistration(noHyphenRegistration, removeHyphen: true);

            return FindPrefixDetail(
                normalisedRegistration,
                prefix => prefix.DecodeNoHyphenRegex.IsMatch(normalisedRegistration)
            );
        }

        private string NormaliseRegistration(string registration, bool removeHyphen)
        {
            var result = (registration ?? "").Trim().ToUpperInvariant();
            if(removeHyphen) {
                result = result.AsciiAlphanumeric();
            }

            return result;
        }

        private IList<RegistrationPrefixDetail> FindPrefixDetail(string normalisedRegistration, Func<RegistrationPrefixDetail, bool> predicate)
        {
            IList<RegistrationPrefixDetail> result = null;

            var buckets = _RegistrationFirstLetterToDetailsMap;
            if(buckets != null && normalisedRegistration.Length > 0) {
                if(buckets.TryGetValue(normalisedRegistration[0], out var bucket)) {
                    result = bucket
                        .Where(prefix => normalisedRegistration.StartsWith(prefix.Prefix) && predicate(prefix))
                        .ToArray();
                }
            }

            return result ?? [];
        }

        private void Initialise()
        {
            if(!_Loaded) {
                Load();
                _Loaded = true;
            }
        }

        private void StartDownloadIfOutOfDate()
        {
            var now = DateTime.UtcNow;

            if(_LastSuccessfulDownloadUtc.AddHours(24) <= now) {
                if(_LastAttemptUtc.AddMinutes(1) <= now) {
                    _LastAttemptUtc = now;
                    ThreadPool.QueueUserWorkItem(DownloadOnBackgroundThread);
                }
            }
        }

        private void DownloadOnBackgroundThread(object unusedState)
        {
            try {
                DownloadAndSaveFile();
                Load();
            } catch(ThreadAbortException) {
                ;
            } catch(Exception ex) {
                _Log.Exception(ex, "Caught exception when downloading registration prefix details");
            }
        }

        private void DownloadAndSaveFile()
        {
            var content = Task.Run(() =>
                _HttpClient.Shared.GetStringAsync(_Settings.LatestValue.Url)
            ).Result;
            if(!String.IsNullOrEmpty(content)) {
                _FileSystem.WriteAllText(
                    LocalCopyFileName(),
                    content
                );
                _LastSuccessfulDownloadUtc = DateTime.UtcNow;
            }
        }

        private string LocalCopyFileName()
        {
            return _FileSystem.Combine(
                _WorkingFolder.Folder,
                "reg-prefixes.csv"
            );
        }

        private void Load()
        {
            var fullPath = LocalCopyFileName();
            if(_FileSystem.FileExists(fullPath)) {
                string[] contentLines = null;
                lock(_SyncLock) {
                    contentLines = _FileSystem.ReadAllLines(fullPath);
                }
                ParseContentLines(contentLines);
            }
        }

        private void ParseContentLines(IEnumerable<string> contentLines)
        {
            var regPrefixes = new List<RegistrationPrefixDetail>();
            IDictionary<string, int> headers = null;
            foreach(var line in contentLines) {
                var chunks = _CsvParser.ParseLineToChunks(line);
                if(chunks.Count >= 6) {
                    if(headers == null) {
                        headers = _CsvParser.ExtractOrdinals(chunks);
                    } else {
                        regPrefixes.Add(new RegistrationPrefixDetail(
                            prefix:                 chunks[headers["Prefix"]],
                            countryISO2:            chunks[headers["CountryISO2"]],
                            hasHyphen:              chunks[headers["HasHyphen"]] == "1",
                            decodeFullRegex:        chunks[headers["DecodeFullRegex"]],
                            decodeNoHyphenRegex:    chunks[headers["DecodeNoHyphenRegex"]],
                            formatTemplate:         chunks[headers["FormatTemplate"]]
                        ));
                    }
                }
            }

            var bucketMap = regPrefixes
                .GroupBy(r => r.Prefix[0])
                .ToDictionary(r => r.Key, r => r.ToArray());

            lock(_SyncLock) {
                _RegistrationFirstLetterToDetailsMap = bucketMap;
            }
        }
    }
}
