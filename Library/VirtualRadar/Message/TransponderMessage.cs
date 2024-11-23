// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Message
{
    /// <summary>
    /// The internal representation of the content of a transmission from an aircraft's transponder.
    /// Almost all properties are nullable, if a property is null then it was not transmitted by the
    /// aircraft or present on the feed.
    /// </summary>
    public class TransponderMessage
    {
        /// <summary>
        /// The unique identifier of the aircraft that transmitted the message. For most sources this
        /// will be the ICAO24. Mandatory. Cannot change over the lifetime of the session.
        /// </summary>
        public int AircraftId { get; }

        /// <summary>
        /// The ICAO24 of the aircraft that transmitted the message.
        /// </summary>
        public Icao24? Icao24 { get; set; }

        /// <summary>
        /// The date and time that the message was received.
        /// </summary>
        public DateTimeOffset MessageReceived { get; set; }

        /// <summary>
        /// True if the aircraft is not a real aircraft.
        /// </summary>
        public bool IsFakeAircraft { get; set; }

        /// <summary>
        /// True if lookups should be suppressed for this aircraft.
        /// </summary>
        public bool SuppressLookup => IsFakeAircraft;

        /// <summary>
        /// The callsign transmitted by the aircraft.
        /// </summary>
        public string Callsign { get; set; }

        /// <summary>
        /// True if <see cref="Callsign"/> is not null and it was gleaned from a raw message that might hold
        /// a callsign, but might not. If a callsign has already been established from a more reliable source
        /// then ignore the <see cref="Callsign"/> presented here.
        /// </summary>
        /// <remarks>
        /// This is set when the callsign was extracted from a Comm-B message where the first byte of the
        /// payload was 0x20, which *could* indicate that it is a BDS2,0 message. Other Comm-B messages
        /// might inadvertently set the same, so the callsign could be garbage.
        /// </remarks>
        public bool? CallsignIsSuspect { get; set; }

        /// <summary>
        /// The altitude transmitted by the aircraft.
        /// </summary>
        public int? AltitudeFeet { get; set; }

        /// <summary>
        /// If <see cref="AltitudeFeet"/> is not null then this is the type of altitude transmitted.
        /// </summary>
        public AltitudeType? AltitudeType { get; set; }

        /// <summary>
        /// The vertical rate of ascent (+ve) or descent (-ve).
        /// </summary>
        public int? VerticalRateFeetPerMinute { get; set; }

        /// <summary>
        /// If <see cref="VerticalRateFeetPerMinute"/> is not null then this is the type of altitude
        /// instrument that the vertical rate is taken from.
        /// </summary>
        public AltitudeType? VerticalRateType { get; set; }

        /// <summary>
        /// The ground speed transmitted by the aircraft.
        /// </summary>
        public float? GroundSpeedKnots { get; set; }

        /// <summary>
        /// If <see cref="GroundSpeedKnots"/> is not null then this is the type of speed transmitted.
        /// </summary>
        public SpeedType? GroundSpeedType { get; set; }

        /// <summary>
        /// The ground track in degrees where 0 is north and 180 is south. Values greater than 360 must
        /// be normalised to be modulo 360.
        /// </summary>
        public float? GroundTrackDegrees { get; set; }

        /// <summary>
        /// True if <see cref="GroundTrackDegrees"/> is not null and the source of the message indicates
        /// that the aircraft is transmitting its heading, not its ground track.
        /// </summary>
        public bool? GroundTrackIsHeading { get; set; }

        /// <summary>
        /// The aircraft's location in WGS84 coordinates.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// The aircraft's base 8 squawk code as a base 10 number.
        /// </summary>
        /// <remarks><para>
        /// Squawk codes are a 4-digit base 8 number on the aircraft (each digit has a value from 0 to 7).
        /// The squawk code 0010 is the number eight represented in base 8: (0 * 8^3) + (0 * 8^2) +
        /// (1 * 8^1) + (0 * 8^0). Technically we should convert squawks from base 8 before storing them
        /// in this field.
        /// </para><para>
        /// However, storing squawks as base 8 numbers makes life awkward when formatting output and
        /// debugging. Base 10 has all the digits needed to represent a base 8 number, so for the sake of
        /// convenience the base 8 squawks are intentionally misrepresented here as a base 10 number.
        /// </para><para>
        /// In short, if the aircraft transmits squawk 0010 then this property will be 10, not 8.
        /// </para>
        /// </remarks>
        public int? Squawk { get; set; }

        /// <summary>
        /// True if the aicraft is transmitting ident.
        /// </summary>
        public bool? IdentActive { get; set; }

        /// <summary>
        /// True if the aircraft says that its wheels are on the ground.
        /// </summary>
        public bool? OnGround { get; set; }

        /// <summary>
        /// True if <see cref="SignalLevel"/> is significant. All feeds should endeavour to set or clear this
        /// value if they know for certain that signal level is or is not sent by the feed. Do not leave it at
        /// null if you know that the signal level is not sent.
        /// </summary>
        public bool? SignalLevelSent { get; set; }

        /// <summary>
        /// The signal level, if known. The meaning and range of the signal level is not known, it is a
        /// property of the feed or receiver and can only be considered within the context of that feed or
        /// recevier.
        /// </summary>
        public int? SignalLevel { get; set; }

        /// <summary>
        /// True if the mssage was generated by a TIS-B ground station.
        /// </summary>
        public bool? IsTisb { get; set; }

        /// <summary>
        /// Set if the information seen on the message infers that that aircraft has a transponder with
        /// a given minimum level of capabilities equipped.
        /// </summary>
        public TransponderType? TransponderType { get; set; }

        /// <summary>
        /// The target altitude in feet set on the aircraft's auto-pilot or FMS.
        /// </summary>
        public int? TargetAltitudeFeet { get; set; }

        /// <summary>
        /// The target heading in degrees set on the aircraft's auto-pilot or FMS.
        /// </summary>
        public float? TargetHeadingDegrees { get; set; }

        /// <summary>
        /// The pressure setting in millibars set on the aircraft's auto-pilot or FMS.
        /// </summary>
        public float? PressureSettingMillibars { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="aircraftId"></param>
        public TransponderMessage(int aircraftId)
        {
            AircraftId = aircraftId;
        }
    }
}
