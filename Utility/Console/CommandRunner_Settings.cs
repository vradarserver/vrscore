// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using VirtualRadar.Configuration;

namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_Settings(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        ISettingsStorage _SettingsStorage,
        ISettingsConfiguration _SettingsConfig,
        Options _Options,
        HeaderService _Header
        #pragma warning restore IDE1006
    ) : CommandRunner
    {
        public override async Task<bool> Run()
        {
            await _Header.OutputCopyright();
            await _Header.OutputTitle("Settings");
            await _Header.OutputOptions(
                ("Update", _Options.Update.ToString())
            );

            var configExists = File.Exists(_SettingsStorage.SettingsLocation());
            Ansi.WriteLine(
                "Using ",
                Ansi.WhiteBold,
                _SettingsStorage.SettingsLocation(),
                Ansi.White,
                " (",
                configExists ? Ansi.White : Ansi.RedBold,
                configExists ? "exists" : "does not exist",
                Ansi.White,
                ")"
            );

            if(_Options.Update) {
                UpdateSettings();
            } else {
                DumpSettings();
            }

            return true;
        }

        private void UpdateSettings()
        {
            Console.WriteLine($"Resaving settings with new entries, this will not overwrite existing entries");
            _SettingsStorage.SaveChanges();
        }

        private void DumpSettings()
        {
            Console.WriteLine("Configuration content:");
            Console.WriteLine();

            var keysToTypes = _SettingsConfig.GetTopLevelTypesMap();
            foreach(var kvp in keysToTypes.OrderBy(r => r.Key, StringComparer.InvariantCultureIgnoreCase)) {
                var keyName = kvp.Key;
                var keyTypes = kvp.Value;

                Ansi.WriteLine(
                    "+ ",
                    Ansi.WhiteBold,
                    keyName
                );

                foreach(var keyType in keyTypes) {
                    var isConfigured = _SettingsStorage.IsConfigured(keyType);
                    var value = _SettingsStorage.LatestValue(keyType);
                    var serialised = _SettingsStorage.ToString(value);

                    Ansi.WriteLine(
                        "    + ",
                        Ansi.YellowBold,
                        keyType.Name,
                        Ansi.White,
                        " (",
                        isConfigured ? Ansi.White : Ansi.RedBold,
                        isConfigured ? "from config" : "default, not in config",
                        Ansi.White,
                        ")"
                    );
                    foreach(var line in serialised.Split([ "\r\n", "\n" ], StringSplitOptions.None)) {
                        Console.WriteLine($"      {line}");
                    }
                }
            }
        }
    }
}
