// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using WindowProcessor;

namespace VirtualRadar.Utility.Terminal
{
    class AircraftListWindow : Window
    {
        class AircraftTableRow
        {
            public string Icao24 { get; set; }

            public string Msgs { get; set; }

            public string Callsign { get; set; }

            public string Squawk { get; set; }

            public string Latitude { get; set; }

            public string Longitude { get; set; }

            public string Registration { get; set; }

            public string ModelIcao { get; set; }
        }

        private IAircraftList _AircraftList;
        private volatile Table<AircraftTableRow> _AircraftTable;
        private Point _CountTrackedPoint;
        private Timer _Timer;

        public long CountChunksSeen { get; set; }

        public AircraftListWindow(
            IAircraftList aircraftList
        )
        {
            _AircraftList = aircraftList;
        }

        protected override void Initialise()
        {
            _AircraftTable = new([
                    new("ICAO24", 6),
                    new("Msgs", 4, Alignment.Right),
                    new("Callsign", 8),
                    new("Squawk", 6, Alignment.Centre),
                    new("Latitude", 11, Alignment.Right),
                    new("Longitude", 11, Alignment.Right),
                    new("Reg", 10),
                    new("Model", 8),
                ], row => [
                    row.Icao24,
                    row.Msgs,
                    row.Callsign,
                    row.Squawk,
                    row.Latitude,
                    row.Longitude,
                    row.Registration,
                    row.ModelIcao
                ],
                new(BorderStyle.Double), new(BorderStyle.Single)
            );

            Console.CursorVisible = false;
            ClearScreen(ConsoleColor.Blue);
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("Tracking ");
            _CountTrackedPoint = Point.Current;
            Console.WriteLine();

            _AircraftTable.DrawHeadingInto(this);

            _Timer = new Timer(_ => Redraw(), null, 1, 1000);
        }

        protected override void DoRedraw()
        {
            if(!_CancellationToken.IsCancellationRequested) {
                var set = _AircraftList
                    ?.ToArray()
                    .OrderBy(r => r.Icao24)
                    .Select(r => new AircraftTableRow() {
                        Icao24 =        r.Icao24.ToString(),
                        Msgs =          r.CountMessagesReceived.Value.ToString("N0"),
                        Callsign =      r.Callsign ?? "",
                        Squawk =        Format.Squawk.Base10AsBase8(r.Squawk),
                        Latitude =      Format.Latitude.IsoRounded(r.Location.Value?.Latitude),
                        Longitude =     Format.Longitude.IsoRounded(r.Location.Value?.Longitude),
                        Registration =  r.Registration ?? "",
                        ModelIcao =     r.ModelIcao ?? "",
                    })
                    .ToArray()
                    ?? [];

                Position = _CountTrackedPoint;
                Write($"{set.Length:N0} from {CountChunksSeen:N0} chunks");
                ClearToEndOfLine();

                _AircraftTable.DrawBody(set, Console.WindowHeight - 5);

                Position = new(0, Console.WindowHeight - 1);
                Write($"Press Q to quit");
            }
        }

        protected override void HandleKeyPress(ConsoleKeyInfo keyInfo)
        {
            if(keyInfo.Key == ConsoleKey.Q) {
                Cancel();
                _InitialColors.Apply();
                Console.CursorVisible = true;
            }
            base.HandleKeyPress(keyInfo);
        }
    }
}
