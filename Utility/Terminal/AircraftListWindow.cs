// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;

namespace VirtualRadar.Utility.Terminal
{
    class AircraftListWindow
    {
        class AircraftDetail
        {
            public string Icao24 { get; set; }

            public string Msgs { get; set; }

            public string Callsign { get; set; }

            public string Squawk { get; set; }

            public string Latitude { get; set; }

            public string Longitude { get; set; }
        }

        private (int Left,int Top) _CountTrackedPoint;
        private readonly Timer _Timer;

        public IAircraftList AircraftList { get; set; }

        public long CountChunksSeen { get; set; }

        public AircraftListWindow()
        {
            Console.Clear();
            Console.Write("Count tracked: ");
            _CountTrackedPoint = (Console.CursorLeft, Console.CursorTop);
            Console.WriteLine();

            WriteTableLine([
                $"{"ICAO",-6}",
                $"{"Msgs",6}",
                $"{"Callsign",-8}",
                $"{"Squawk",-6}",
                $"{"Latitude",12}",
                $"{"Longitude",12}",
            ]);

            _Timer = new Timer(_ => RefreshContent(), null, 1000, 1000);
        }

        private void WriteTableLine(string[] columns)
        {
            var buffer = new StringBuilder();
            buffer.Append('│');
            foreach(var column in columns) {
                buffer.Append($"{column}│");
            }
            WriteAndClearToEOL(buffer.ToString());
        }

        private void WriteAndClearToEOL(string line)
        {
            if(Console.CursorTop < Console.WindowHeight - 2) {
                var widthRemaining = Console.WindowWidth - Console.CursorLeft;
                if(line.Length > widthRemaining) {
                    line = line[..widthRemaining];
                }
                Console.Write(line);
                var padding = widthRemaining - line.Length;
                if(padding > 0) {
                    var builder = new StringBuilder();
                    for(var idx = 0;idx < padding;++idx) {
                        builder.Append(' ');
                    }
                    Console.Write(builder.ToString());
                }
                Console.WriteLine();
            }
        }

        private static bool _Refreshing;
        public void RefreshContent()
        {
            if(!_Refreshing && AircraftList != null) {
                _Refreshing = true;
                try {
                    Console.CursorVisible = false;
                    var set = AircraftList
                        .ToArray()
                        .OrderBy(r => r.Icao24)
                        .Select(r => new AircraftDetail() {
                            Icao24 =    r.Icao24.ToString(),
                            Msgs =      r.CountMessagesReceived.Value.ToString("N0"),
                            Callsign =  r.Callsign ?? "",
                            Squawk =    Format.Squawk.Base10AsBase8(r.Squawk),
                            Latitude =  Format.Latitude.IsoRounded(r.Location.Value?.Latitude),
                            Longitude = Format.Longitude.IsoRounded(r.Location.Value?.Longitude),
                        })
                        .ToArray();

                    Console.SetCursorPosition(_CountTrackedPoint.Left, _CountTrackedPoint.Top);
                    WriteAndClearToEOL($"{set.Length:N0} from {CountChunksSeen:N0} chunks");
                    Console.WriteLine();
                    foreach(var aircraft in set) {
                        WriteTableLine([
                            $"{aircraft.Icao24,-6}",
                            $"{aircraft.Msgs,6}",
                            $"{aircraft.Callsign,-8}",
                            $" {aircraft.Squawk,-5}",
                            $"{aircraft.Latitude,12}",
                            $"{aircraft.Longitude,12}",
                        ]);
                    }
                    for(var lineNumber = Console.CursorTop;lineNumber < Console.WindowHeight;++lineNumber) {
                        WriteTableLine([
                            $"{"",-6}",
                            $"{"",6}",
                            $"{"",-8}",
                            $"{"",-6}",
                            $"{"",12}",
                            $"{"",12}",
                        ]);
                    }
                    Console.WriteLine($"Press Q to quit");
                } finally {
                    Console.CursorVisible = true;
                    _Refreshing = false;
                }
            }
        }
    }
}
