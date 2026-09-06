// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.CommandLine;

namespace VirtualRadar.Utility.CLIConsole
{
    public class Command_ShowModules(
        #pragma warning disable IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
        IModuleInformationService _ModuleInfo
        #pragma warning restore IDE1006 // VS2022/26 .editorconfig bugged for primary ctors
    ) : CommonCommand
    {
        public async Task<bool> RunAsync()
        {
            var loadedModules = _ModuleInfo.LoadedModules;
            var rejectedModules = _ModuleInfo.RejectedModules;

            await WriteLineAsync("Loaded Modules");
            await WriteLineAsync("--------------");
            for(var idx = 0;idx < loadedModules.Length;++idx) {
                var module = loadedModules[idx];
                if(idx != 0) {
                    await WriteLineAsync();
                }
                await WriteLineAsync($"{module.Manifest.ModuleName}");
                await WriteLineAsync($"    Filename: {module.FileName}");
                await WriteLineAsync($"    Priority: {module.ModuleInstance.Priority}");
                await WriteLineAsync($"    Versions: {module.Manifest.MinimumSupportedVirtualRadarVersion} to {module.Manifest.MaximumSupportedVirtualRadarVersion}");
            }

            await WriteLineAsync();
            await WriteLineAsync("Rejected Modules");
            await WriteLineAsync("----------------");
            if(rejectedModules.Length == 0) {
                await WriteLineAsync("None");
            }
            for(var idx = 0;idx < rejectedModules.Length;++idx) {
                var reject = rejectedModules[idx];
                if(idx != 0) {
                    await WriteLineAsync();
                }
                await WriteLineAsync($"Filename: {reject.FileName}");
                await WriteLineAsync($"Reason:   {reject.Reason}");
            }

            return true;
        }
    }
}
