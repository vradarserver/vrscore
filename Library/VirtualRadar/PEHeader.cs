// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Buffers;
using System.Reflection;

namespace VirtualRadar
{
    /// <summary>
    /// Utility methods for portable executable headers. Note that all .NET Core executables
    /// have PE headers when they contain JIT'd binaries. None of this will work for AOT.
    /// </summary>
    public static class PEHeader
    {
        private const int _HeaderPointerOffset = 60;    // Offset from start of file to a 32-bit offset to the start of the PE header
        private const int _LinkerTimestampOffset = 8;   // Offset from start of header to linker timestamp

        /// <summary>
        /// Positions the stream at the start of the PE header.
        /// </summary>
        /// <param name="exeFileStream"></param>
        /// <returns></returns>
        public static void PositionAtHeaderStart(FileStream exeFileStream)
        {
            if(exeFileStream != null) {
                exeFileStream.Position = GetPEHeaderOffset(exeFileStream);
            }
        }

        /// <summary>
        /// Retrieves the location of the PE header in the EXE file stream passed
        /// in. This will change the Position of the stream.
        /// </summary>
        /// <param name="exeFileStream"></param>
        /// <returns></returns>
        public static int GetPEHeaderOffset(FileStream exeFileStream)
        {
            var result = -1;

            if(exeFileStream != null) {
                exeFileStream.Position = _HeaderPointerOffset;
                using(var rental = MemoryPool<byte>.Shared.Rent(4)) {
                    var buffer = rental.Memory.Span;
                    exeFileStream.ReadAtLeast(buffer, 4);
                    result = BitConverter.ToInt32(buffer);
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts the build date from the header of the assembly passed across. Note that linkers will not
        /// emit a build date in the header if they have been configured to generate deterministic
        /// executables. Either turn that stuff off or set the build date yourself (assuming that it's not
        /// been signed).
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static DateTimeOffset ExtractBuildDate(Assembly assembly) => ExtractBuildDate(assembly?.Location);

        /// <summary>
        /// Extracts the build date from the header of the executable file whose fully-pathed filename has
        /// been submitted. Note that linkers will not emit a build date in the header if they have been
        /// configured to generate deterministic executables. Either turn that stuff off or set the build date
        /// yourself (assuming that it's not been signed).
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static DateTimeOffset ExtractBuildDate(string fileName)
        {
            DateTimeOffset result = default;

            if(!String.IsNullOrEmpty(fileName) && File.Exists(fileName)) {
                using(var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                    PositionAtHeaderStart(stream);
                    stream.Position += _LinkerTimestampOffset;

                    using(var rental = MemoryPool<byte>.Shared.Rent(4)) {
                        var buffer = rental.Memory.Span;
                        stream.ReadAtLeast(buffer, 4);

                        var timestamp = BitConverter.ToInt32(buffer);
                        result = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        result = result.AddSeconds(timestamp);
                    }
                }
            }

            return result;
        }

    }
}
