﻿// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using Moq;
using VirtualRadar;

namespace Tests.Mocks
{
    public class MockFileSystem : IFileSystem
    {
        public Mock<IFileSystem> Mock { get; } = MockHelper.CreateMock<IFileSystem>();

        public bool UseFileContent = true;

        public HashSet<string> CaseSensitiveFolders { get; } = new();

        public Dictionary<string, byte[]> CaseSensitiveFileContent { get; } = new();

        public static string NormalisePathDos(string path) => NormalisePath(path, '\\', '/');

        public static string NormalisePathUnix(string path) => NormalisePath(path, '/', '\\');

        private static string NormalisePath(string path, char osSeparator, char otherOSSeperator)
        {
            if(path?.Length > 0 && path[^1] != osSeparator) {
                if(path[^1] == otherOSSeperator) {
                    path = path[..^1];
                }
                path += osSeparator;
            }
            return path;
        }

        /// <summary>
        /// Adds a folder to <see cref="CaseSensitiveFolders"/>
        /// </summary>
        /// <param name="path"></param>
        public void AddFolder(string path)
        {
            if(!CaseSensitiveFolders.Contains(path)) {
                CaseSensitiveFolders.Add(path);
            }
        }

        /// <summary>
        /// Removes the folder if there are no files in <see cref="CaseSensitiveFileContent"/>
        /// that refer to it.
        /// </summary>
        /// <param name="path"></param>
        public void RemoveFolder(string path)
        {
            var dosPath = NormalisePathDos(path);
            var unixPath = NormalisePathUnix(path);

            if(CaseSensitiveFolders.Contains(path) && !CaseSensitiveFileContent.Any(r => r.Key.StartsWith(dosPath) || r.Key.StartsWith(unixPath))) {
                CaseSensitiveFolders.Remove(path);
            }
        }

        /// <summary>
        /// Adds a file to <see cref="CaseSensitiveFileContent"/> and its folder
        /// to <see cref="CaseSensitiveFolders"/>.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="content"></param>
        public void AddFileContent(string fullPath, byte[] content)
        {
            CaseSensitiveFileContent[fullPath] = content;
            AddFolder(Path.GetDirectoryName(fullPath));
        }

        /// <summary>
        /// Adds a file to <see cref="CaseSensitiveFileContent"/> and its folder
        /// to <see cref="CaseSensitiveFolders"/>.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        public void AddFileContent(string fullPath, string content, Encoding encoding = null)
        {
            AddFileContent(
                fullPath,
                (encoding ?? Encoding.UTF8).GetBytes(content)
            );
        }

        /// <summary>
        /// Adds a file containing the lines of text passed across.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="lines"></param>
        /// <param name="encoding"></param>
        /// <param name="lineSeparator"></param>
        public void AddFileContent(string fullPath, IEnumerable<string> lines, Encoding encoding = null, string lineSeparator = null)
        {
            AddFileContent(
                fullPath,
                String.Join(lineSeparator ?? "\r\n", lines),
                encoding
            );
        }

        /// <summary>
        /// Returns the content of a file as a string.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string GetFileContentAsString(string fullPath, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetString(
                CaseSensitiveFileContent[fullPath]
            );
        }

        /// <summary>
        /// Removes the file content and optionally, if there are no other files in the folder,
        /// the file's folder.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="removeFolderIfEmpty"></param>
        public void RemoveFile(string fullPath, bool removeFolderIfEmpty = true)
        {
            if(CaseSensitiveFileContent.ContainsKey(fullPath)) {
                CaseSensitiveFileContent.Remove(fullPath);
                if(removeFolderIfEmpty) {
                    RemoveFolder(Path.GetDirectoryName(fullPath));
                }
            }
        }

        private void ThrowIfFileDirectoryNotFound(string fullPath)
        {
            if(!CaseSensitiveFolders.Contains(Path.GetDirectoryName(fullPath))) {
                throw new DirectoryNotFoundException();
            }
        }

        private void ThrowIfFileNotFound(string fullPath)
        {
            if(!CaseSensitiveFileContent.ContainsKey(fullPath)) {
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// This is not mocked, it just passes through to Path.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string Combine(params string[] paths) => Path.Combine(paths);

        public bool IsValidFileNameResult { get; set; } = true;
        public bool IsValidFileName(string fileName) => IsValidFileNameResult;

        public bool IsValidPathNameResult { get; set; } = true;
        public bool IsValidPathName(string path) => IsValidPathNameResult;

        /// <summary>
        /// This is not mocked, it just passes through to Path.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public string GetDirectory(string fullPath) => Path.GetDirectoryName(fullPath);

        public string GetFileName(string fullPath) => Path.GetFileName(fullPath);

        public string GetFileNameWithoutExtension(string fullPath) => Path.GetFileNameWithoutExtension(fullPath);

        public string GetExtension(string fullPath) => Path.GetExtension(fullPath);

        public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
        {
            if(!UseFileContent) {
                Mock.Object.CopyFile(sourceFileName, destFileName, overwrite);
            } else {
                ThrowIfFileNotFound(sourceFileName);
                if(CaseSensitiveFileContent.ContainsKey(destFileName) && !overwrite) {
                    throw new IOException();
                }
                AddFileContent(destFileName, CaseSensitiveFileContent[sourceFileName]);
            }
        }

        public bool CreateDirectoryIfNotExists(string path)
        {
            bool result;

            if(!UseFileContent) {
                result = Mock.Object.CreateDirectoryIfNotExists(path);
            } else {
                result = !CaseSensitiveFolders.Contains(path);
                if(result) {
                    AddFolder(path);
                }
            }

            return result;
        }

        public void DeleteFile(string fileName)
        {
            if(!UseFileContent) {
                Mock.Object.DeleteFile(fileName);
            } else {
                ThrowIfFileNotFound(fileName);
                CaseSensitiveFileContent.Remove(fileName);
            }
        }

        public bool DirectoryExists(string path)
        {
            return !UseFileContent
                ? Mock.Object.DirectoryExists(path)
                : CaseSensitiveFolders.Contains(path);
        }

        public bool FileExists(string fileName)
        {
            return !UseFileContent
                ? Mock.Object.FileExists(fileName)
                : CaseSensitiveFileContent.ContainsKey(fileName);
        }

        public long FileSize(string fileName)
        {
            long result;
            if(!UseFileContent) {
                result = Mock.Object.FileSize(fileName);
            } else {
                ThrowIfFileNotFound(fileName);
                result = CaseSensitiveFileContent[fileName].Length;
            }

            return result;
        }

        public void MoveFile(string sourceFileName, string destFileName, bool overwrite)
        {
            ThrowIfFileNotFound(sourceFileName);
            if(FileExists(destFileName) && !overwrite) {
                throw new IOException();
            }

            AddFileContent(destFileName, CaseSensitiveFileContent[sourceFileName]);
            RemoveFile(sourceFileName);
        }

        public Stream OpenFileStream(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            switch(fileMode) {
                case FileMode.CreateNew:
                    if(FileExists(fileName)) {
                        throw new IOException();
                    }
                    break;
                case FileMode.Create:
                    if(FileExists(fileName)) {
                        CaseSensitiveFileContent[fileName] = Array.Empty<byte>();
                    }
                    break;
                case FileMode.Open:
                    if(!FileExists(fileName)) {
                        throw new FileNotFoundException();
                    }
                    break;
                case FileMode.OpenOrCreate:
                    if(!FileExists(fileName)) {
                        CaseSensitiveFileContent[fileName] = Array.Empty<byte>();
                    }
                    break;
                case FileMode.Truncate:
                    ThrowIfFileNotFound(fileName);
                    goto case FileMode.Create;
            }

            if(!CaseSensitiveFileContent.ContainsKey(fileName)) {
                CaseSensitiveFileContent[fileName] = Array.Empty<byte>();
            }

            var result = new ByteStream(
                CaseSensitiveFileContent[fileName],
                writeCallback: newContent => CaseSensitiveFileContent[fileName] = newContent.ToArray()
            );

            return result;
        }

        public byte[] ReadAllBytes(string fileName)
        {
            byte[] result;
            if(!UseFileContent) {
                result = Mock.Object.ReadAllBytes(fileName);
            } else {
                ThrowIfFileNotFound(fileName);
                result = CaseSensitiveFileContent[fileName];
            }

            return result;
        }

        public async Task<byte[]> ReadAllBytesAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] result;
            if(!UseFileContent) {
                result = await Mock.Object.ReadAllBytesAsync(fileName, cancellationToken);
            } else {
                ThrowIfFileNotFound(fileName);
                result = CaseSensitiveFileContent[fileName];
            }

            return result;
        }

        public string[] ReadAllLines(string fileName)
        {
            return !UseFileContent
                ? Mock.Object.ReadAllLines(fileName)
                : GetContentAsLines(fileName);
        }

        public async Task<string[]> ReadAllLinesAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return !UseFileContent
                ? await Mock.Object.ReadAllLinesAsync(fileName, cancellationToken)
                : GetContentAsLines(fileName);
        }

        public string ReadAllText(string fileName, Encoding encoding = null)
        {
            return !UseFileContent
                ? Mock.Object.ReadAllText(fileName, encoding)
                : GetContentAsText(fileName);
        }

        public async Task<string> ReadAllTextAsync(string fileName, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return !UseFileContent
                ? await Mock.Object.ReadAllTextAsync(fileName, encoding, cancellationToken)
                : GetContentAsText(fileName);
        }

        private string GetContentAsText(string fileName, Encoding encoding = null)
        {
            ThrowIfFileNotFound(fileName);
            return (encoding ?? Encoding.UTF8).GetString(
                CaseSensitiveFileContent[fileName]
            );
        }

        private string[] GetContentAsLines(string fileName, Encoding encoding = null)
        {
            return GetContentAsText(fileName, encoding)
                .Split(new string[] { "\r\n", "\n", }, StringSplitOptions.None);
        }

        public void WriteAllBytes(string fileName, byte[] bytes)
        {
            if(!UseFileContent) {
                Mock.Object.WriteAllBytes(fileName, bytes);
            } else {
                AddFileContent(fileName, bytes);
            }
        }

        public async Task WriteAllBytesAsync(string fileName, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(!UseFileContent) {
                await Mock.Object.WriteAllBytesAsync(fileName, bytes, cancellationToken);
            } else {
                AddFileContent(fileName, bytes);
            }
        }

        public void WriteAllLines(string fileName, IEnumerable<string> contents)
        {
            if(!UseFileContent) {
                Mock.Object.WriteAllLines(fileName, contents);
            } else {
                AddFileContent(fileName, Encoding.UTF8.GetBytes(String.Join(Environment.NewLine, contents)));
            }
        }

        public async Task WriteAllLinesAsync(string fileName, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(!UseFileContent) {
                await Mock.Object.WriteAllLinesAsync(fileName, contents, cancellationToken);
            } else {
                AddFileContent(fileName, Encoding.UTF8.GetBytes(String.Join(Path.DirectorySeparatorChar, contents)));
            }
        }

        public void WriteAllText(string fileName, string text, Encoding encoding = null)
        {
            if(!UseFileContent) {
                Mock.Object.WriteAllText(fileName, text, encoding);
            } else {
                AddFileContent(fileName, (encoding ?? Encoding.UTF8).GetBytes(text));
            }
        }

        public async Task WriteAllTextAsync(string fileName, string text, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(!UseFileContent) {
                await Mock.Object.WriteAllTextAsync(fileName, text, encoding, cancellationToken);
            } else {
                AddFileContent(fileName, (encoding ?? Encoding.UTF8).GetBytes(text));
            }
        }
    }
}
