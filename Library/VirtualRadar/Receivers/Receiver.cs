// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.AircraftLists;
using VirtualRadar.Connection;
using VirtualRadar.Feed;
using VirtualRadar.Message;
using VirtualRadar.StandingData;

namespace VirtualRadar.Receivers
{
    /// <summary>
    /// Brings together a source of messages, a decoded feed of those messages and an aircraft list.
    /// </summary>
    public class Receiver : IReceiver
    {
        private ILog _Log;
        private IAircraftOnlineLookupService _AircraftLookupService;
        private IStandingDataManager _StandingDataManager;

        /// <inheritdoc/>
        public ReceiverOptions Options { get; }

        /// <inheritdoc/>
        public int Id => Options.Id;

        /// <inheritdoc/>
        public string Name => Options.Name;

        /// <inheritdoc/>
        public bool Enabled => Options.Enabled;

        /// <inheritdoc/>
        public bool Hidden => Options.Hidden;

        private CancellationTokenSource _ConnectionCancellationTokenSource = new();
        /// <inheritdoc/>
        public CancellationToken ConnectionCancellationToken => _ConnectionCancellationTokenSource.Token;

        /// <inheritdoc/>
        public IReceiveConnector Connector { get; }

        /// <inheritdoc/>
        public IFeedDecoder FeedDecoder { get; }

        /// <inheritdoc/>
        public IAircraftList AircraftList { get; }

        private long _CountPacketsReceived;
        /// <inheritdoc/>
        public long CountPacketsReceived => _CountPacketsReceived;

        private long _CountMessagesReceived;
        /// <inheritdoc/>
        public long CountMessagesReceived => _CountMessagesReceived;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connector"></param>
        /// <param name="feedDecoder"></param>
        /// <param name="aircraftList"></param>
        /// <param name="log"></param>
        /// <param name="aircraftLookupService"></param>
        /// <param name="standingDataRepository"></param>
        internal Receiver(
            ReceiverOptions options,
            IReceiveConnector connector,
            IFeedDecoder feedDecoder,
            IAircraftList aircraftList,
            ILog log,
            IAircraftOnlineLookupService aircraftLookupService,
            IStandingDataManager standingDataManager
        )
        {
            Options = options;
            Connector = connector;
            FeedDecoder = feedDecoder;
            AircraftList = aircraftList;

            _Log = log;
            _StandingDataManager = standingDataManager;
            _AircraftLookupService = aircraftLookupService;

            _AircraftLookupService.LookupCompleted += AircraftLookupService_LookupCompleted;
            Connector.PacketReceived += Connector_PacketReceived;
            FeedDecoder.MessageReceived += FeedDecoder_MessageReceived;
            FeedDecoder.LookupReceived += FeedDecoder_LookupReceived;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Receiver() => Dispose(false);

        /// <inheritdoc/>
        public override string ToString() => Name ?? "";

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _AircraftLookupService.LookupCompleted -= AircraftLookupService_LookupCompleted;
                Connector.PacketReceived -= Connector_PacketReceived;
                FeedDecoder.MessageReceived -= FeedDecoder_MessageReceived;
            }
        }

        /// <inheritdoc/>
        public void Start()
        {
            Task.Run(() => Connector.OpenAsync(ConnectionCancellationToken));
        }

        private Task LookupStandingDataDetailsInBackground(BatchedLookupOutcome<LookupByIcaoOutcome> args)
        {
            try {
                var foundLookups = new List<LookupByIcaoOutcome>();

                LookupByIcaoOutcome createFailedOutcome(Icao24 icao24) => new(icao24, success: false);
                void addSuccessOutcome(LookupByIcaoOutcome outcome)
                {
                    if(outcome.Success) {
                        foundLookups.Add(outcome);
                    }
                }

                foreach(var icaoLookup in args.Found) {
                    var standingDataLookup = createFailedOutcome(icaoLookup.Icao24);

                    LookupAircraftTypeInformation(standingDataLookup, icaoLookup);
                    LookupCodeBlockInformation(standingDataLookup, icaoLookup);

                    addSuccessOutcome(standingDataLookup);
                }

                foreach(var icaoLookup in args.Missing) {
                    var standingDataLookup = createFailedOutcome(icaoLookup.Icao24);

                    LookupCodeBlockInformation(standingDataLookup, icaoLookup);

                    addSuccessOutcome(standingDataLookup);
                }

                if(foundLookups.Count != 0) {
                    AircraftList.ApplyLookup(
                        new BatchedLookupOutcome<LookupByIcaoOutcome>(foundLookups, [])
                    );
                }
            } catch(Exception ex) {
                _Log.Exception(ex, $"Caught exception looking up standing data details after an ICAO24 details lookup");
            }

            return Task.CompletedTask;
        }

        private Task LookupCallsignRouteInBackground(int aircraftId, string callsign)
        {
            try {
                var route = _StandingDataManager.FindRoute(callsign);
                if(route != null) {
                    AircraftList.ApplyLookup(new LookupByAircraftIdOutcome(aircraftId, success: true) {
                        Route = route,
                    });
                }
            } catch(Exception ex) {
                _Log.Exception(ex, $"Caught exception looking up standing data route information for callsign \"{callsign}\"");
            }

            return Task.CompletedTask;
        }

        private void LookupAircraftTypeInformation(LookupByIcaoOutcome standingDataLookup, LookupByIcaoOutcome icaoLookup)
        {
            if(!String.IsNullOrEmpty(icaoLookup.ModelIcao)) {
                var model = _StandingDataManager.FindAircraftType(icaoLookup.ModelIcao);
                if(model != null) {
                    standingDataLookup.Success =                    true;
                    standingDataLookup.EnginePlacement ??=          model.EnginePlacement;
                    standingDataLookup.EngineType ??=               model.EngineType;
                    standingDataLookup.NumberOfEngines ??=          model.Engines;
                    standingDataLookup.Species ??=                  model.Species;
                    standingDataLookup.WakeTurbulenceCategory ??=   model.WakeTurbulenceCategory;
                }
            }
        }

        private void LookupCodeBlockInformation(LookupByIcaoOutcome standingDataLookup, LookupByIcaoOutcome icaoLookup)
        {
            var codeBlock = _StandingDataManager.FindCodeBlock(icaoLookup.Icao24);
            if(codeBlock != null) {
                standingDataLookup.Success =            true;
                standingDataLookup.Icao24Country ??=    codeBlock.ModeSCountry;
                standingDataLookup.IsMilitary ??=       codeBlock.IsMilitary;
            }
        }

        private void AircraftLookupService_LookupCompleted(object sender, BatchedLookupOutcome<LookupByIcaoOutcome> args)
        {
            Task.Run(() => LookupStandingDataDetailsInBackground(args));
            AircraftList.ApplyLookup(args);
        }

        private void Connector_PacketReceived(object sender, ReadOnlyMemory<byte> args)
        {
            Interlocked.Increment(ref _CountPacketsReceived);
            FeedDecoder.ParseFeedPacket(args);
        }

        private void FeedDecoder_LookupReceived(object sender, LookupByAircraftIdOutcome lookupOutcome)
        {
            // Feeds that provide their own lookups are expected to provide all lookup details.
            // We do not add our own lookups.
            AircraftList.ApplyLookup(lookupOutcome);
        }

        private void FeedDecoder_MessageReceived(object sender, TransponderMessage args)
        {
            Interlocked.Increment(ref _CountMessagesReceived);
            var outcome = AircraftList.ApplyMessage(args);

            if(!args.SuppressLookup && (args.Icao24?.IsValid ?? false)) {
                if(outcome.ChangeSet?.Callsign != null) {
                    Task.Run(() => LookupCallsignRouteInBackground(outcome.Message.AircraftId, outcome.Message.Callsign));
                }
                if(outcome.AddedAircraft) {
                    _AircraftLookupService?.Lookup(args.Icao24.Value);
                }
            }
        }
    }
}
