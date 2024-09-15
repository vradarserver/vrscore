﻿// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using VirtualRadar.Connection;
using VirtualRadar.Feed;
using VirtualRadar.Feed.BaseStation;
using VirtualRadar.Feed.Recording;
using VirtualRadar.IO;
using VirtualRadar.Message;
using WindowProcessor;

namespace VirtualRadar.Utility.Terminal
{
    /// <summary>
    /// Just a quickie object to plug together a basic feed pipeline and show the results.
    /// </summary>
    class TempRunner(
        Options                         _Options,
        IFeedDecoderFactory             _FeedDecoderFactory,
        IServiceProvider                _ServiceProvider,
        IAircraftOnlineLookupService    _AircraftLookupService,
        IConnectorFactory                _ConnectorFactory
    )
    {
        public async Task Run()
        {
            var feedDecoder = _FeedDecoderFactory.Build(new BaseStationFeedDecoderOptions());
            using(var scope = _ServiceProvider.CreateScope()) {
                var aircraftList = scope.ServiceProvider.GetRequiredService<IAircraftList>();
                var connector = OpenConnector();
                var cancelSource = new CancellationTokenSource();

                try {
                    _AircraftLookupService.LookupCompleted += (_,batchOutcome) => {
                        aircraftList.ApplyLookup(batchOutcome);
                    };

                    var aircraftListWindow = scope.ServiceProvider.GetRequiredService<AircraftListWindow>();
                    aircraftListWindow.AircraftList = aircraftList;

                    var windowEventLoopTask = aircraftListWindow.EventLoop(cancelSource);

                    connector.ConnectionStateChanged += (_,_) => aircraftListWindow.ConnectionState = connector.ConnectionState.ToString();

                    connector.LastExceptionChanged += (_,_) => aircraftListWindow.LastConnectorException = connector.LastException;

                    connector.PacketReceived += (_, packet) => {
                        ++aircraftListWindow.CountPacketsSeen;
                        feedDecoder.ParseFeedPacket(packet);
                    };

                    feedDecoder.MessageReceived += (_, message) => {
                        var applyOutcome = aircraftList.ApplyMessage(message);
                        if(applyOutcome.AddedAircraft) {
                            _AircraftLookupService.Lookup(message.Icao24);
                        }
                    };

                    ControlC.SuppressCancelBehaviour = true;

                    await connector.OpenAsync(cancelSource.Token);
                    await windowEventLoopTask;
                } catch(OperationCanceledException) {
                    Console.Clear();
                }
            }
        }

        private IReceiveConnector OpenConnector()
        {
            return String.IsNullOrEmpty(_Options.FileName)
                ? OpenNetworkConnector()
                : OpenRecordingConnector();
        }

        private IReceiveConnector OpenNetworkConnector()
        {
            Console.WriteLine($"Connecting to BaseStation feed on {_Options.Address}:{_Options.Port}");
            if(!IPAddress.TryParse(_Options.Address, out var address)) {
                OptionsParser.Usage($"{_Options.Address} is not a valid IP address");
            }

            return _ConnectorFactory.Build<IReceiveConnector>(new TcpPullConnectorSettings() {
                Address =   address,
                Port =      _Options.Port,
            });
        }

        private IReceiveConnector OpenRecordingConnector()
        {
            Console.WriteLine($"Replaying feed recording from {_Options.FileName}");

            return _ConnectorFactory.Build<IReceiveConnector>(new RecordingPlaybackConnectorOptions() {
                RecordingFileName = _Options.FileName,
                PlaybackSpeed =     _Options.PlaybackSpeed,
            });
        }
    }
}
