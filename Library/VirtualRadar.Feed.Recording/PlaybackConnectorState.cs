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
using VirtualRadar.Connection;
using VirtualRadar.Extensions;

namespace VirtualRadar.Feed.Recording
{
    class PlaybackConnectorState(RecordingPlaybackConnector _Parent) : CancellableState
    {
        public FileStream FileStream;

        public RecordingReader Reader;

        public PlaybackTimeSync PlaybackSync;

        public Task PumpTask;

        protected override void TearDownState()
        {
            var exceptions = new List<Exception>();

            if(_Parent._State == this) {
                exceptions.Capture(() => _Parent.ConnectionState = ConnectionState.Closing);
            }

            CancelEverything(exceptions);

            if(PumpTask != null) {
                exceptions.Capture(() => PumpTask.Wait(5000));
                PumpTask = null;
            }

            if(Reader != null) {
                exceptions.Capture(() => Reader.TearDownStream());
                Reader = null;
            }

            if(FileStream != null) {
                exceptions.Capture(() => FileStream.Dispose());
                FileStream = null;
            }

            TearDownCancellationTokens(exceptions);

            if(_Parent._State == this) {
                exceptions.Capture(() => _Parent.ConnectionState = ConnectionState.Closed);
            }

            if(exceptions.Count > 0) {
                throw new ConnectionTearDownException(exceptions);
            }
        }
    }
}
