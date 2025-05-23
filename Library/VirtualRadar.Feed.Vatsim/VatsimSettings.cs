﻿// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Configuration;

namespace VirtualRadar.Feed.Vatsim
{
    /// <summary>
    /// The global settings for VATSIM decoding.
    /// </summary>
    /// <param name="RefreshIntervalSeconds">The number of seconds between each fetch of VATSIM data.</param>
    /// <param name="StatusUrl">The URL to download status data from.</param>
    /// <param name="RefreshStatusHours">The number of hours to wait between refreshes of status.</param>
    /// <param name="AssumeSlowAircraftAreOnGround">
    /// Infer "on ground" state from the speed of the aircraft.
    /// </param>
    /// <param name="SlowAircraftThresholdSpeedKnots">
    /// The speed in knots under which the aircraft is assumed to be on the ground when <paramref
    /// name="AssumeSlowAircraftAreOnGround"/> is true.
    /// </param>
    /// <param name="InferModelFromModelType">
    /// Infer the manufacturer and model names from the aircraft type.
    /// </param>
    /// <param name="ShowInvalidRegistrations">
    /// True if invalid registrations are to be suppressed from display.
    /// </param>
    [Settings("Vatsim")]
    public record VatsimSettings(
        int RefreshIntervalSeconds = 16,
        string StatusUrl = "https://status.vatsim.net/status.json",
        int RefreshStatusHours = 1,
        bool AssumeSlowAircraftAreOnGround = true,
        int SlowAircraftThresholdSpeedKnots = 40,
        bool InferModelFromModelType = true,
        bool ShowInvalidRegistrations = false
    )
    {
    }
}
