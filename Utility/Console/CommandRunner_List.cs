// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Feed;

namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_List : CommandRunner
    {
        private Options _Options;
        private HeaderService _Header;
        private BootService _BootService;
        private IFeedFormatFactoryService _FeedFormatService;

        public CommandRunner_List(Options options, HeaderService header, BootService bootService, IFeedFormatFactoryService feedFormatService)
        {
            _Options = options;
            _Header = header;
            _BootService = bootService;
            _FeedFormatService = feedFormatService;
        }

        public override async Task<bool> Run()
        {
            await _Header.OutputCopyright();
            await _Header.OutputTitle("List");
            await _Header.OutputOptions(
                ("Entity",  _Options.ListEntity.ToString())
            );
            await WriteLine();

            switch (_Options.ListEntity) {
                case ListEntity.FeedFormats:    DumpFeedFormats(); break;
                default:                        throw new NotImplementedException();
            }

            return true;
        }

        private void DumpFeedFormats()
        {
            _BootService.Initialise();

            foreach(var config in _FeedFormatService.GetAllConfigs().OrderBy(r => r.Name(CultureInfo.CurrentCulture))) {
                Console.WriteLine($"Id [{config.Id}] Invariant Name [{config.Name(CultureInfo.InvariantCulture)}] Local Name [{config.Name(CultureInfo.CurrentCulture)}]");
            }
        }
    }
}
