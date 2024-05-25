// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Newtonsoft.Json;
using VirtualRadar.Collections;

namespace VirtualRadar.Services
{
    /// <summary>
    /// Default implementation of <see cref="IWebAddressManager"/>.
    /// </summary>
    class WebAddressManager : IWebAddressManager
    {
        private WorkingFolder _WorkingFolder;
        private IFileSystem _FileSystem;

        /// <summary>
        /// Controls writes to the fields.
        /// </summary>
        private readonly object _SyncLock = new object();

        /// <summary>
        /// True if the file has been loaded.
        /// </summary>
        private volatile bool _Loaded;

        /// <summary>
        /// The content of the address file. Never change the collection - always take a reference to it when reading
        /// outside of a lock, and always overwrite it instead of changing it.
        /// </summary>
        private volatile Dictionary<string, string> _Store = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public string AddressFileFullPath => _FileSystem.Combine(
            _WorkingFolder.Folder,
            "WebAddresses.json"
        );

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebAddressManager(
            IFileSystem fileSystem,
            WorkingFolder workingFolder
        )
        {
            _FileSystem = fileSystem;
            _WorkingFolder = workingFolder;
        }

        /// <inheritdoc/>
        public string RegisterAddress(string name, string address, IList<string> oldAddresses = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(address);

            name = name.Trim();
            address = address.Trim();

            if(!Uri.TryCreate(address, UriKind.Absolute, out _)) {
                throw new ArgumentOutOfRangeException(nameof(address), $"{address} is not a valid URL");
            }

            var extant = LookupAddress(name);
            var changeCollection = extant == null;

            if(!changeCollection && extant != address) {
                // If the existing address isn't the same as the one we want to record then we want to preserve it
                // on the understanding that it's a custom address entered by the user.
                //
                // If the extant and new address differ only by case then we overwrite the extant with the new
                // address - this allows for fixes to old addresses where the wrong case was used.
                //
                // If the extant is known to be an old address that the program or plugin used in the past but no
                // longer supports then we are also allowed to change it.

                changeCollection = String.Equals(extant, address, StringComparison.InvariantCultureIgnoreCase);
                if(!changeCollection && oldAddresses != null) {
                    changeCollection = oldAddresses.Any(oldAddress => String.Equals(address, oldAddress, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            if(changeCollection) {
                lock(_SyncLock) {
                    var newCollection = ShallowCollectionCopier.Copy(_Store);
                    newCollection[name] = address;
                    _Store = newCollection;
                    Save();
                }
            }

            return LookupAddress(name);
        }

        /// <inheritdoc/>
        public string LookupAddress(string name)
        {
            Load();

            var collection = _Store;
            collection.TryGetValue(name, out var result);

            return result;
        }

        /// <summary>
        /// Loads the address file from disk if it has not already been loaded.
        /// </summary>
        private void Load()
        {
            if(!_Loaded) {
                lock(_SyncLock) {
                    if(!_Loaded) {
                        _Loaded = true;

                        if(_FileSystem.FileExists(AddressFileFullPath)) {
                            var jsonText = _FileSystem.ReadAllText(AddressFileFullPath);

                            Dictionary<string, string> rawDictionary;
                            try {
                                rawDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
                            } catch(Newtonsoft.Json.JsonException) {
                                BackupOldFileAsBad();
                                Save();
                                rawDictionary = [];
                            }

                            var newStore = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach(var kvp in rawDictionary) {
                                var key = (kvp.Key ?? "").Trim();
                                var value = (kvp.Value ?? "").Trim();

                                if(key != "" && value != "" && Uri.TryCreate(value, UriKind.Absolute, out _)) {
                                    newStore.Add(key, value);
                                }
                            }

                            _Store = newStore;
                        }
                    }
                }
            }
        }

        private void BackupOldFileAsBad()
        {
            lock(_SyncLock) {
                if(_FileSystem.FileExists(AddressFileFullPath)) {
                    var backupFileName = $"{AddressFileFullPath}-bad";
                    _FileSystem.CopyFile(AddressFileFullPath, backupFileName, overwrite: true);
                }
            }
        }

        private void Save()
        {
            lock(_SyncLock) {
                var collection = _Store;
                var jsonText = JsonConvert.SerializeObject(collection, Formatting.Indented);
                _FileSystem.WriteAllText(AddressFileFullPath, jsonText);
            }
        }
    }
}
