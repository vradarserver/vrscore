﻿// Copyright © 2018 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.TileServer
{
    /// <summary>
    /// The interface for classes that can read and write downloaded tile server settings.
    /// </summary>
    [Lifetime(Lifetime.Singleton)]
    public interface IDownloadedTileServerSettingsStorage
    {
        /// <summary>
        /// Returns true if the settings have been downloaded at least once.
        /// </summary>
        /// <returns></returns>
        bool DownloadedSettingsFileExists();

        /// <summary>
        /// Loads and returns the entire set of tile server settings. If no settings could be found on disk
        /// then a default set is returned.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<DownloadedTileServerSettings> Load();

        /// <summary>
        /// Saves a set of downloaded settings.
        /// </summary>
        /// <param name="settings"></param>
        void SaveDownloadedSettings(IEnumerable<DownloadedTileServerSettings> settings);

        /// <summary>
        /// Saves a readme file detailing how to create a custom tile server settings file.
        /// </summary>
        void CreateReadme();
    }
}
