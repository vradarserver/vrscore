// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Terminal.Gui;

namespace VirtualRadar.Utility.Terminal
{
    class AircraftListWindow : Window
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

        private TextField _CountAircraftField;
        private TableView _AircraftTableView;
        private readonly TableDataBuilder<AircraftDetail> _AircraftListTableBuilder;
        private readonly Timer _Timer;

        public IAircraftList AircraftList { get; set; }

        public AircraftListWindow()
        {
            X = 0;
            Y = 1;
            Width = Dim.Fill();
            Height = Dim.Fill();

            var countAircraftLabel = new Label("Aircraft tracked") {
                X = 0,
                Y = 0,
            };
            Add(countAircraftLabel);
            _CountAircraftField = new() {
                X = Pos.Right(countAircraftLabel) + 1,
                Y = Pos.Top(countAircraftLabel),
                ReadOnly = true,
                TextAlignment = TextAlignment.Right,
                Width = 10,
            };
            Add(_CountAircraftField);

            _AircraftTableView = new() {
                X = Pos.Left(countAircraftLabel),
                Y = Pos.Top(countAircraftLabel) + 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                FullRowSelect = true,
                NullSymbol = "",
            };
            Add(_AircraftTableView);

            _AircraftListTableBuilder = new TableDataBuilder<AircraftDetail>() {
                ColumnDefinitions = {
                    new(r => r.Icao24,          "ICAO"),
                    new(r => r.Msgs,            "Msgs")         { CellAlignment = TextAlignment.Right, },
                    new(r => r.Callsign,        "Callsign"),
                    new(r => r.Squawk,          "Squawk"),
                    new(r => r.Latitude,        "Latitude")     { CellAlignment = TextAlignment.Right, },
                    new(r => r.Longitude,       "Longitude")    { CellAlignment = TextAlignment.Right, },
                },
            };

            RefreshContent();

            _Timer = new Timer(_ => RefreshContent(), null, 0, 250);
        }

        private static bool _Refreshing;
        public void RefreshContent()
        {
            if(!_Refreshing && AircraftList != null) {
                _Refreshing = true;
                try {
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
                    _CountAircraftField.Text = set.Length.ToString("N0");
                    var dataTable = _AircraftListTableBuilder.BuildDataTable(set);
                    _AircraftTableView.Table = dataTable;
                    _AircraftListTableBuilder.ApplyStyling(_AircraftTableView);
                    Redraw(Bounds);
                } finally {
                    _Refreshing = false;
                }
            }
        }
    }
}
