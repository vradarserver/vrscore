// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar
{
    /// <summary>
    /// Manages the working folder.
    /// </summary>
    [Lifetime(Lifetime.Singleton)]
    public class WorkingFolder
    {
        private IFileSystem _FileSystem;
        private long _CountFolderAccessed;

        private string _Folder = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VirtualRadarCore"
        );

        /// <summary>
        /// Returns the working folder, creating it if it does not already exist.
        /// </summary>
        public string Folder
        {
            get {
                if(_CountFolderAccessed == 0) {
                    _FileSystem.CreateDirectoryIfNotExists(_Folder);
                }
                Interlocked.Increment(ref _CountFolderAccessed);
                return _Folder;
            }
        }

        /// <summary>
        /// Returns the number of times <see cref="Folder"/> has been read.
        /// </summary>
        public long CountFolderAccessed => _CountFolderAccessed;

        public WorkingFolder(IFileSystem fileSystem)
        {
            _FileSystem = fileSystem;
        }

        /// <inheritdoc/>
        public override string ToString() => _Folder ?? "";

        /// <summary>
        /// Changes <see cref="Folder"/>.
        /// </summary>
        /// <param name="newFolder"></param>
        /// <returns>True if <see cref="Folder"/> has not yet been accessed.</returns>
        public bool ChangeFolder(string newFolder)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(newFolder);

            var result = _CountFolderAccessed == 0;
            _FileSystem.CreateDirectoryIfNotExists(newFolder);
            _Folder = newFolder;

            return result;
        }
    }
}
