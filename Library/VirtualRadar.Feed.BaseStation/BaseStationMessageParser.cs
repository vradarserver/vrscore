// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Extensions.Options;
using VirtualRadar.Configuration;

namespace VirtualRadar.Feed.BaseStation
{
    public class BaseStationMessageParser
    {
        private ApplicationSettings _ApplicationSettings;

        public BaseStationMessageParser(IOptions<ApplicationSettings> applicationSettings)
        {
            _ApplicationSettings = applicationSettings.Value;
        }

        public BaseStationMessage FromFeed(ReadOnlyMemory<byte> bytes)
        {
            return FromFeed(bytes, _ApplicationSettings.LocalTimeZone.GetUtcOffset(DateTime.UtcNow));
        }

        public BaseStationMessage FromFeed(ReadOnlyMemory<byte> bytes, TimeSpan localTimeOffset)
        {
            BaseStationMessage result = null;

            if(bytes.Length > 0) {
                var messageLine = Encoding.ASCII.GetString(bytes.Span);
                result = Translate(messageLine, localTimeOffset);
            }

            return result;
        }

        public BaseStationMessage Translate(string text)
        {
            return Translate(text, _ApplicationSettings.LocalTimeZone.GetUtcOffset(DateTime.UtcNow));
        }

        public BaseStationMessage Translate(string text, TimeSpan localTimeOffset)
        {
            BaseStationMessage result = null;

            if(!String.IsNullOrEmpty(text)) {
                var parts = text.Split(',');
                if(parts.Length >= 22) {
                    result = new();
                    for(var c = 0;c < parts.Length;c++) {
                        var chunk = parts[c].Trim();
                        if(!String.IsNullOrEmpty(chunk)) {
                            switch(c) {
                                case 0:     result.MessageType = chunk.ToUpper(); break;
                                case 1:     result.TransmissionType = chunk.ToUpper(); break;
                                case 2:     result.SessionId = ParseInt(chunk); break;
                                case 3:     result.AircraftId = ParseInt(chunk); break;
                                case 4:     result.Icao24 = chunk; break;
                                case 5:     result.FlightId = ParseInt(chunk); break;
                                case 6:     result.MessageGenerated = ParseDate(chunk, localTimeOffset); break;
                                case 7:     result.MessageGenerated = ParseTime(result.MessageGenerated, chunk); break;
                                case 8:     result.MessageLogged = ParseDate(chunk, localTimeOffset); break;
                                case 9:     result.MessageLogged = ParseTime(result.MessageLogged, chunk); break;
                                case 10:    result.Callsign = result.IsAircraftMessage ? chunk : null; break;
                                case 11:    result.Altitude = ParseInt(chunk); break;
                                case 12:    result.GroundSpeed = ParseFloat(chunk); break;
                                case 13:    result.Track = ParseFloat(chunk); break;
                                case 14:    result.Latitude = ParseDouble(chunk); break;
                                case 15:    result.Longitude = ParseDouble(chunk); break;
                                case 16:    result.VerticalRate = ParseInt(chunk); break;
                                case 17:    result.Squawk = ParseInt(chunk); if(result.Squawk == 0) result.Squawk = null; break;
                                case 18:    result.SquawkHasChanged = ParseBool(chunk); break;
                                case 19:    result.Emergency = ParseBool(chunk); break;
                                case 20:    result.IdentActive = ParseBool(chunk); break;
                                case 21:    result.OnGround = ParseBool(chunk); break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static int ParseInt(string chunk) => int.Parse(chunk, CultureInfo.InvariantCulture);

        private static float ParseFloat(string chunk) => float.Parse(chunk, CultureInfo.InvariantCulture);

        private static double ParseDouble(string chunk) => double.Parse(chunk, CultureInfo.InvariantCulture);

        private static bool ParseBool(string chunk) => chunk != "0";

        // Note that locale settings can play havoc with delimiters sent from BaseStation. I've given up using DateTime.Parse and
        // I'm just plucking out the numbers by hand...
        private static DateTimeOffset ParseDate(string chunk, TimeSpan localTimeOffset)
        {
            if(chunk.Length != 10) {
                throw new InvalidOperationException($"{chunk} doesn't look like a valid date");
            }
            var year = int.Parse(chunk.Substring(0, 4));
            var month = int.Parse(chunk.Substring(5,2));
            var day = int.Parse(chunk.Substring(8, 2));

            return new DateTimeOffset(year, month, day, 0, 0, 0, localTimeOffset);
        }

        // See notes against ParseDate for explanation of parser
        private static DateTimeOffset ParseTime(DateTimeOffset date, string chunk)
        {
            if(chunk.Length != 12) {
                throw new InvalidOperationException($"{chunk} doesn't look like a valid time");
            }
            var hour = int.Parse(chunk.Substring(0, 2));
            var minute = int.Parse(chunk.Substring(3, 2));
            var second = int.Parse(chunk.Substring(6, 2));
            var millisecond = int.Parse(chunk.Substring(9, 3));

            return new DateTimeOffset(
                date.Year,
                date.Month,
                date.Day,
                hour,
                minute,
                second,
                millisecond,
                date.Offset
            );
        }
    }
}
