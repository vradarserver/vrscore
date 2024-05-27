// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Format
{
    public static class Duration
    {
        public static string AgoAway(DateTimeOffset now, DateTimeOffset then, CultureInfo culture, bool shortUnits)
        {
            // TODO: This needs i18n

            TimeSpan diff;
            string suffix;

            if(now >= then) {
                diff = now - then;
                suffix = "ago";
            } else {
                diff = then - now;
                suffix = "away";
            }

            string format(double value, string singleShort, string singleLong, string pluralShort, string pluralLong)
            {
                var floor = (int)Math.Floor(value);
                var buffer = new StringBuilder(floor.ToString("N0", culture));
                buffer.Append(' ');
                if(floor == 1) {
                    buffer.Append(shortUnits ? singleShort : singleLong);
                } else {
                    buffer.Append(shortUnits ? pluralShort : pluralLong);
                }
                buffer.Append(' ');
                buffer.Append(suffix);
                return buffer.ToString();
            }

            if(diff.TotalSeconds < 60) {
                return format(diff.TotalSeconds, "sec", "second", "secs", "seconds");
            } else if(diff.TotalMinutes < 60) {
                return format(diff.TotalMinutes, "min", "minute", "mins", "minutes");
            } else if(diff.TotalHours < 24) {
                return format(diff.TotalHours, "hr", "hour", "hrs", "hours");
            }

            return format(diff.TotalDays, "day", "day", "days", "days");
        }
    }
}
