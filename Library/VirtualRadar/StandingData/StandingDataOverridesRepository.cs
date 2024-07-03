// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Extensions;

namespace VirtualRadar.StandingData
{
    class StandingDataOverridesRepository(
        IWorkingFolder _WorkingFolder,
        IFileSystem _FileSystem
    ) : IStandingDataOverridesRepository
    {
        private readonly object _SyncLock = new();

        private volatile Dictionary<Icao24, CodeBlock> _CustomCodeBlocks;

        private string _CodeBlocksFileFullyPathed => _FileSystem.Combine(_WorkingFolder.Folder, "LocalAircraft.txt");

        /// <inheritdoc/>
        public CodeBlock CodeBlockOverrideFor(Icao24 icao24)
        {
            CodeBlock result = null;

            var cache = _CustomCodeBlocks;
            cache?.TryGetValue(icao24, out result);

            return result;
        }

        /// <inheritdoc/>
        public void Load()
        {
            lock(_SyncLock) {
                if(_CustomCodeBlocks != null) {
                    _CustomCodeBlocks = null;
                }
                if(_FileSystem.FileExists(_CodeBlocksFileFullyPathed)) {
                    LoadCodeBlocks();
                }
            }
        }

        private void LoadCodeBlocks()
        {
            var newCodeBlocks = new List<CustomCodeBlock>();

            string country = null;
            var lineNumber = 0;
            foreach(var line in _FileSystem.ReadAllLines(_CodeBlocksFileFullyPathed)) {
                ++lineNumber;
                var text = line.Trim();

                var commentPosn = text.IndexOf('#');
                if(commentPosn != -1) {
                    text = text[..commentPosn].Trim();
                }

                if(text != "") {
                    if(text[0] == '[' && text[^1] == ']') {
                        country = text[1..^1].Trim();
                    } else {
                        var chunks = text.Split(
                            StringExtensions.AllAsciiWhiteSpace,
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        if(chunks.Length > 0) {
                            if(String.IsNullOrEmpty(country)) {
                                // TODO: log.WriteLine("Missing country at line {0} of local codeblock override", lineNumber);
                            } else {
                                var icao = chunks[0];
                                if(!Icao24.TryParse(icao, out var icao24)) {
                                    // TODO: log.WriteLine("Invalid ICAO {0} at line {1} of local codeblock override", icao, lineNumber);
                                    continue;
                                }

                                var isMilitary = chunks.Length > 1;
                                if(isMilitary) {
                                    var mil = chunks[1].Trim();
                                    isMilitary = mil.Equals("MIL", StringComparison.InvariantCultureIgnoreCase);
                                    if(!isMilitary && !mil.Equals("CIV", StringComparison.InvariantCultureIgnoreCase)) {
                                        // TODO: log.WriteLine("Invalid military/civilian designator '{0}' - must be one of 'mil' or 'civ' at line {1} of local codeblock override", mil, lineNumber);
                                        continue;
                                    }
                                }

                                newCodeBlocks.Add(new() {
                                    Icao24 = icao24,
                                    IsMilitary = isMilitary,
                                    Country = country,
                                });
                            }
                        }
                    }
                }
            }

            if(newCodeBlocks.Count > 0) {
                var newCache = new Dictionary<Icao24, CodeBlock>();
                foreach(var codeBlock in newCodeBlocks) {
                    newCache[codeBlock.Icao24] = new() {
                        Country =       codeBlock.Country,
                        IsMilitary =    codeBlock.IsMilitary,
                    };
                }
                _CustomCodeBlocks = newCache;
            }
        }
    }
}
