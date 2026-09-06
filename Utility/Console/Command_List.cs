// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Extensions.Hosting;
using VirtualRadar.CommandLine;
using VirtualRadar.Feed;

namespace VirtualRadar.Utility.CLIConsole
{
    public class Command_List(
        #pragma warning disable IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
        IHost _Host,
        HeaderService _Header,
        IFeedFormatFactoryService _FeedFormatService
        #pragma warning restore IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
    ) : CommonCommand
    {
        public ListEntity Entity { get; set; }

        public async Task<bool> RunAsync()
        {
            await _Header.OutputCopyrightAsync();
            await _Header.OutputTitleAsync("List");
            await _Header.OutputOptionsAsync(
                ("Entity",  Entity.ToString())
            );
            await WriteLineAsync();

            switch(Entity) {
                case ListEntity.FeedFormats:    await DumpFeedFormatsAsync(); break;
                default:                        throw new NotImplementedException();
            }

            return true;
        }

        private async Task DumpFeedFormatsAsync()
        {
            _Host.StartVirtualRadarServer();
            try {
                foreach(var config in _FeedFormatService.GetAllConfigs().OrderBy(r => r.Name(CultureInfo.CurrentCulture))) {
                    await WriteLineAsync($"Id [{config.Id}] Invariant Name [{config.Name(CultureInfo.InvariantCulture)}] Local Name [{config.Name(CultureInfo.CurrentCulture)}]");
                }
            } finally {
                _Host.StopVirtualRadarServer();
            }
        }
    }
}
