﻿// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_Open(Options _Options, IWorkingFolder _WorkingFolder) : CommandRunner
    {
        public override async Task<bool> Run()
        {
            bool result = false;

            switch(_Options.OpenEntity) {
                case OpenEntity.WorkingFolder:  result = await OpenWorkingFolder(); break;
                default:                        throw new NotImplementedException();
            }

            return result;
        }

        private async Task<bool> OpenWorkingFolder()
        {
            await WriteLine($"Opening {_WorkingFolder.Folder}");

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Process.Start(new ProcessStartInfo() {
                    FileName = "explorer.exe",
                    ArgumentList = { _WorkingFolder.Folder, },
                    UseShellExecute = true,
                });
            } else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Process.Start(new ProcessStartInfo() {
                    FileName = "open",
                    ArgumentList = { _WorkingFolder.Folder, },
                    UseShellExecute = true,
                });
            } else {
                Process.Start("xdg-open", _WorkingFolder.Folder);
            }

            return true;
        }
    }
}
