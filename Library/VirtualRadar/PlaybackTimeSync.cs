// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar
{
    /// <summary>
    /// Manages the synchronisation between a series of events that take place
    /// at intervals after the start of a playback.
    /// </summary>
    public class PlaybackTimeSync
    {
        /// <summary>
        /// The time that playback started.
        /// </summary>
        public DateTime PlaybackStartedUtc { get; }

        /// <summary>
        /// The time of the last recording event from the start of playback. This indicates
        /// how much recording time has passed, not how much real time. It is the last
        /// millisecond offset plus the playback start time.
        /// </summary>
        public DateTime PlaybackTimeUtc { get; private set; }

        /// <summary>
        /// The real UTC time of the last playback recording event. This is used to adjust
        /// the pause time to account for program delays between events - e.g. if the gap
        /// between recorded offsets is 10ms and the gap between calls for those two offsets
        /// is 2ms of real time then we only pause for 8ms, not the full 10.
        /// </summary>
        public DateTime LastEventTimeUtc { get; private set; }

        /// <summary>
        /// The playback speed. A speed of 0 turns playback synchronisation off, 1 attempts
        /// to keep real time, 2 doubles the playback speed, 0.5 halves it and so on.
        /// </summary>
        public double PlaybackSpeed { get; } = 1.0;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PlaybackTimeSync() : this(DateTime.UtcNow, 1.0)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="playbackSpeed"></param>
        public PlaybackTimeSync(double playbackSpeed) : this(DateTime.UtcNow, playbackSpeed)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="playbackStartedUtc"></param>
        /// <param name="playbackSpeed"></param>
        public PlaybackTimeSync(DateTime playbackStartedUtc, double playbackSpeed)
        {
            PlaybackStartedUtc = playbackStartedUtc;
            LastEventTimeUtc = playbackStartedUtc;
            PlaybackSpeed = playbackSpeed;
        }

        /// <summary>
        /// Given an offset in milliseconds from the start of playback, this blocks the current thread until a
        /// sufficient interval has passed between <see cref="PlaybackTimeUtc"/> and the time represented by
        /// this offset. It updates <see cref="PlaybackTimeUtc"/> before returning.
        /// </summary>
        /// <param name="eventOffsetUtc"></param>
        public void WaitForEvent(uint millisecondsFromStart)
        {
            var millisecondsUntilEvent = CalculateEventWaitMilliseconds(millisecondsFromStart);
            var adjustedMilliseconds = AdjustForPlaybackSpeed(millisecondsUntilEvent);
            if(adjustedMilliseconds > 0) {
                Thread.Sleep(adjustedMilliseconds);
            }
            LastEventTimeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Given an offset in milliseconds from the start of playback, this delays the thread until a
        /// sufficient interval has passed between <see cref="PlaybackTimeUtc"/> and the time represented by
        /// this offset. It updates <see cref="PlaybackTimeUtc"/> before returning.
        /// </summary>
        /// <param name="eventOffsetUtc"></param>
        /// <param name="cancellationToken"></param>
        public async Task WaitForEventAsync(uint millisecondsFromStart, CancellationToken cancellationToken)
        {
            var millisecondsUntilEvent = CalculateEventWaitMilliseconds(millisecondsFromStart);
            var adjustedMilliseconds = AdjustForPlaybackSpeed(millisecondsUntilEvent);
            if(adjustedMilliseconds > 0) {
                await Task.Delay(adjustedMilliseconds, cancellationToken);
            }
            LastEventTimeUtc = DateTime.UtcNow;
        }

        private double CalculateEventWaitMilliseconds(uint eventMillisecondsFromStart)
        {
            var playbackTime = PlaybackStartedUtc.AddMilliseconds(Math.Max(0, eventMillisecondsFromStart));
            if(playbackTime < PlaybackTimeUtc) {
                playbackTime = PlaybackTimeUtc;
            }

            var previousLastEvent = PlaybackTimeUtc;
            PlaybackTimeUtc = playbackTime;

            var playbackIntervalMilliseconds = (playbackTime - previousLastEvent).TotalMilliseconds;

            var eventTime = DateTime.UtcNow;
            if(eventTime < LastEventTimeUtc) {
                eventTime = LastEventTimeUtc;
            }
            var millisecondsElapsedBetweenCalls = (eventTime - LastEventTimeUtc).TotalMilliseconds;

            return Math.Max(0, playbackIntervalMilliseconds - millisecondsElapsedBetweenCalls);
        }

        private int AdjustForPlaybackSpeed(double millisecondsUntilEvent)
        {
            var playbackSpeed = Math.Max(0.0, PlaybackSpeed);
            return playbackSpeed == 0.0
                ? 0
                : (int)(millisecondsUntilEvent / playbackSpeed);
        }
    }
}
