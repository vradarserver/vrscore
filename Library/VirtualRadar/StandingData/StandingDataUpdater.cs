// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO.Compression;
using VirtualRadar.Extensions;

namespace VirtualRadar.StandingData
{
    class StandingDataUpdater : IStandingDataUpdater
    {
        IWebAddressManager _WebAddressManager;
        IWorkingFolder _WorkingFolder;
        IFileSystem _FileSystem;
        IHttpClientService _HttpClientService;
        IStandingDataRepository _StandingDataRepository;

        /// <summary>
        /// Non-zero if <see cref="Update"/> is running on a thread somewhere.
        /// </summary>
        private int _UpdateRunning;

        /// <summary>
        /// The filename of the standing data database.
        /// </summary>
        private const string DatabaseFileName = "StandingData.sqb";

        /// <summary>
        /// The full path to <see cref="DatabaseFileName"/>.
        /// </summary>
        private string DatabaseFileFullyPathed => _FileSystem.Combine(_WorkingFolder.Folder, DatabaseFileName);

        /// <summary>
        /// The temporary filename of the standing data database.
        /// </summary>
        private const string DatabaseTempFileName = $"{DatabaseFileName}.tmp";

        /// <summary>
        /// The full path to the temporary database file name.
        /// </summary>
        private string DatabaseTempFileFullyPathed => _FileSystem.Combine(_WorkingFolder.Folder, DatabaseTempFileName);

        /// <summary>
        /// The name of the file that describes the dates and state of the other files.
        /// </summary>
        public const string StateFileName = "FlightNumberCoverage.csv";

        /// <summary>
        /// The full path to <see cref="StateFileName"/>.
        /// </summary>
        private string StateFileFullyPathed => _FileSystem.Combine(_WorkingFolder.Folder, StateFileName);

        /// <summary>
        /// The URL of the file holding the standing data database.
        /// </summary>
        private string DatabaseUrl => _WebAddressManager.RegisterAddress(
            "vrs-sdm-database",
            "http://www.virtualradarserver.co.uk/Files/StandingData.sqb.gz"
        );

        /// <summary>
        /// The name of the file that describes the dates and state of the other files.
        /// </summary>
        public string StateTempFileName = $"{StateFileName}.tmp";

        /// <summary>
        /// The full path to <see cref="StateTempFileName"/>.
        /// </summary>
        private string StateTempFileFullyPathed => _FileSystem.Combine(_WorkingFolder.Folder, StateTempFileName);

        /// <summary>
        /// The URL of the file that describes the dates and state of the other files.
        /// </summary>
        private string StateFileUrl => _WebAddressManager.RegisterAddress(
            "vrs-sdm-state-file",
            "http://www.virtualradarserver.co.uk/Files/FlightNumberCoverage.csv"
        );

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webAddressManager"></param>
        /// <param name="workingFolder"></param>
        /// <param name="fileSystem"></param>
        /// <param name="httpClientService"></param>
        /// <param name="standingDataRepository"></param>
        public StandingDataUpdater(
            IWebAddressManager webAddressManager,
            IWorkingFolder workingFolder,
            IFileSystem fileSystem,
            IHttpClientService httpClientService,
            IStandingDataRepository standingDataRepository
        )
        {
            _WebAddressManager = webAddressManager;
            _WorkingFolder = workingFolder;
            _FileSystem = fileSystem;
            _HttpClientService = httpClientService;
            _StandingDataRepository = standingDataRepository;
        }

        /// <inheritdoc/>
        public async Task<bool> DataIsOld(CancellationToken cancellationToken)
        {
            var stateFilename =  StateFileFullyPathed;

            var result = !_FileSystem.FileExists(stateFilename)
                      || !_FileSystem.FileExists(DatabaseFileFullyPathed);
            if(!result) {
                var remoteContent = await DownloadLines(StateFileUrl, cancellationToken);
                var localContent = _FileSystem.ReadAllLines(stateFilename);
                result = remoteContent.Length < 2 || localContent.Length < 2;
                if(!result) {
                    result = remoteContent[1] != localContent[1];
                }
            }

            return result;
        }

        private async Task<string[]> DownloadLines(string url, CancellationToken cancellationToken)
        {
            var httpClient = _HttpClientService.Shared;
            var fileContent = await httpClient.GetStringAsync(url, cancellationToken);
            return fileContent.SplitIntoLines();
        }

        /// <inheritdoc/>
        public async Task Update(CancellationToken cancellationToken)
        {
            if(Interlocked.Exchange(ref _UpdateRunning, 1) == 0) {
                try {
                    var stateFileName = StateFileFullyPathed;
                    var stateTempName = StateTempFileFullyPathed;
                    var databaseFileName = DatabaseFileFullyPathed;
                    var databaseTempName = DatabaseTempFileFullyPathed;

                    var stateLines = await DownloadLines(StateFileUrl, cancellationToken);
                    var remoteStateChunks = stateLines[1].Split(new char[] { ',' });
                    var localStateChunks = _FileSystem.FileExists(stateFileName)
                        ? _FileSystem.ReadAllLines(stateFileName)
                        : [];

                    var remoteDatabaseChecksum = remoteStateChunks.Length > 7
                        ? remoteStateChunks[7]
                        : "MISSING-REMOTE";
                    var localDatabaseChecksum = localStateChunks.Length > 7
                        ? localStateChunks[7]
                        : "MISSING-LOCAL";
                    var updateState = !_FileSystem.FileExists(databaseFileName)
                                   || remoteDatabaseChecksum != localDatabaseChecksum;
                    updateState = updateState && !cancellationToken.IsCancellationRequested;

                    if(updateState) {
                        await DownloadAndDecompressFile(DatabaseUrl, databaseTempName, cancellationToken);
                        if(!cancellationToken.IsCancellationRequested) {
                            _StandingDataRepository.PauseWhile(() => 
                                _FileSystem.MoveFile(
                                    databaseTempName,
                                    databaseFileName,
                                    overwrite: true
                                )
                            );

                            _FileSystem.WriteAllLines(stateFileName, stateLines);
                        }
                    }
                } finally {
                    Interlocked.Exchange(ref _UpdateRunning, 0);
                }
            }
        }

        private async Task DownloadAndDecompressFile(string databaseUrl, string databaseTempName, CancellationToken cancellationToken)
        {
            _FileSystem.CreateDirectoryIfNotExists(
                _FileSystem.GetDirectory(databaseTempName)
            );

            var httpClient = _HttpClientService.Shared;
            using var remoteStream = await httpClient.GetStreamAsync(databaseUrl, cancellationToken);
            using var gzipStream = new GZipStream(remoteStream, CompressionMode.Decompress);
            using var fileStream = _FileSystem.OpenFileStream(databaseTempName, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read);

            await gzipStream.CopyToAsync(fileStream, cancellationToken);
        }
    }
}
