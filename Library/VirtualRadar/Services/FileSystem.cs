// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;

namespace VirtualRadar.Services
{
    /// <summary>
    /// The default file system provider.
    /// </summary>
    class FileSystem : IFileSystem
    {
        private static string LocalAppDataFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <inheritdoc/>
        public string Combine(params string[] paths) => Path.Combine(paths);

        /// <inheritdoc/>
        public bool IsValidFileName(string fileName)
        {
            return !String.IsNullOrEmpty(fileName)
                && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
        }

        /// <inheritdoc/>
        public bool IsValidPathName(string path)
        {
            return !String.IsNullOrEmpty(path)
                && path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
        }

        /// <inheritdoc/>
        public string GetDirectory(string fullPath) => Path.GetDirectoryName(fullPath);

        /// <inheritdoc/>
        public string GetFileName(string fullPath) => Path.GetFileName(fullPath);

        /// <inheritdoc/>
        public string GetFileNameWithoutExtension(string fullPath) => Path.GetFileNameWithoutExtension(fullPath);

        /// <inheritdoc/>
        public string GetExtension(string fullPath) => Path.GetExtension(fullPath);

        /// <inheritdoc/>
        public void CopyFile(string sourceFileName, string destFileName, bool overwrite) => File.Copy(sourceFileName, destFileName, overwrite);

        /// <inheritdoc/>
        public bool CreateDirectoryIfNotExists(string path) => !DirectoryExists(path) && Directory.CreateDirectory(path) != null;

        /// <inheritdoc/>
        public void DeleteFile(string fileName) => File.Delete(fileName);

        /// <inheritdoc/>
        public bool DirectoryExists(string path) => Directory.Exists(path);

        /// <inheritdoc/>
        public bool FileExists(string fileName) => File.Exists(fileName);

        /// <inheritdoc/>
        public long FileSize(string fileName) => new FileInfo(fileName).Length;

        /// <inheritdoc/>
        public void MoveFile(string sourceFileName, string destFileName, bool overwrite) => File.Move(sourceFileName, destFileName, overwrite);

        /// <inheritdoc/>
        public Stream OpenFileStream(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare) => new FileStream(fileName, fileMode, fileAccess, fileShare);

        /// <inheritdoc/>
        public byte[] ReadAllBytes(string fileName) => File.ReadAllBytes(fileName);

        /// <inheritdoc/>
        public Task<byte[]> ReadAllBytesAsync(string fileName, CancellationToken cancellationToken = default) => File.ReadAllBytesAsync(fileName, cancellationToken);

        /// <inheritdoc/>
        public string[] ReadAllLines(string fileName) => File.ReadAllLines(fileName);

        /// <inheritdoc/>
        public Task<string[]> ReadAllLinesAsync(string fileName, CancellationToken cancellationToken) => File.ReadAllLinesAsync(fileName, cancellationToken);

        /// <inheritdoc/>
        public string ReadAllText(string fileName, Encoding encoding = null) => File.ReadAllText(fileName, encoding ?? Encoding.UTF8);

        /// <inheritdoc/>
        public Task<string> ReadAllTextAsync(string fileName, Encoding encoding = null, CancellationToken cancellationToken = default) => File.ReadAllTextAsync(fileName, encoding ?? Encoding.UTF8, cancellationToken);

        /// <inheritdoc/>
        public void WriteAllBytes(string fileName, byte[] bytes) => File.WriteAllBytes(fileName, bytes);

        /// <inheritdoc/>
        public Task WriteAllBytesAsync(string fileName, byte[] bytes, CancellationToken cancellationToken = default) => File.WriteAllBytesAsync(fileName, bytes, cancellationToken);

        /// <inheritdoc/>
        public void WriteAllLines(string fileName, IEnumerable<string> contents) => File.WriteAllLines(fileName, contents);

        /// <inheritdoc/>
        public Task WriteAllLinesAsync(string fileName, IEnumerable<string> contents, CancellationToken cancellationToken = default) => File.WriteAllLinesAsync(fileName, contents, cancellationToken);

        /// <inheritdoc/>
        public void WriteAllText(string fileName, string text, Encoding encoding = null) => File.WriteAllText(fileName, text, encoding ?? Encoding.UTF8);

        /// <inheritdoc/>
        public Task WriteAllTextAsync(string fileName, string text, Encoding encoding = null, CancellationToken cancellationToken = default) => File.WriteAllTextAsync(fileName, text, encoding ?? Encoding.UTF8, cancellationToken);
    }
}
