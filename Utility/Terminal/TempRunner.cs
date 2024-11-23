// Copyright © 2024 onwards, Andrew Whewell
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
using VirtualRadar.Receivers;
using WindowProcessor;

namespace VirtualRadar.Utility.Terminal
{
    /// <summary>
    /// Just a quickie object to plug together a basic feed pipeline and show the results.
    /// </summary>
    class TempRunner(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        Options                         _Options,
        IServiceProvider                _ServiceProvider,
        IAircraftOnlineLookupService    _AircraftLookupService,
        IReceiverFactory                _ReceiverFactory
        #pragma warning restore IDE1006
    )
    {
        public async Task Run()
        {
            using(var scope = _ServiceProvider.CreateScope()) {
                var receiver = OpenReceiver(scope.ServiceProvider);
                var connector = receiver != null ? null : OpenConnector(scope.ServiceProvider);
                var feedDecoder = receiver != null ? null : CreateFeedDecoder(scope.ServiceProvider);
                var aircraftList = receiver != null ? null : scope.ServiceProvider.GetRequiredService<IAircraftList>();
                var cancelSource = new CancellationTokenSource();

                try {
                    if(receiver == null) {
                        _AircraftLookupService.LookupCompleted += (_,batchOutcome) => {
                            aircraftList.ApplyLookup(batchOutcome);
                        };
                    }

                    var aircraftListWindow = scope.ServiceProvider.GetRequiredService<AircraftListWindow>();
                    aircraftListWindow.AircraftList = receiver?.AircraftList ?? aircraftList;

                    var windowEventLoopTask = aircraftListWindow.EventLoop(cancelSource);

                    var receiverOrConnector = receiver?.Connector ?? connector;

                    receiverOrConnector.ConnectionStateChanged += (_,_) => aircraftListWindow.ConnectionState = receiverOrConnector.ConnectionState.ToString();

                    receiverOrConnector.LastExceptionChanged += (_,_) => aircraftListWindow.LastConnectorException = receiverOrConnector.LastException;

                    receiverOrConnector.PacketReceived += (_, packet) => {
                        ++aircraftListWindow.CountPacketsSeen;
                        if(receiver == null) {
                            feedDecoder.ParseFeedPacket(packet);
                        }
                    };

                    if(receiver == null) {
                        feedDecoder.MessageReceived += (_, message) => {
                            var applyOutcome = aircraftList.ApplyMessage(message);
                            if(applyOutcome.AddedAircraft && message.Icao24 != null) {
                                _AircraftLookupService.Lookup(message.Icao24.Value);
                            }
                        };
                    }

                    ControlC.SuppressCancelBehaviour = true;

                    await receiverOrConnector.OpenAsync(cancelSource.Token);
                    await windowEventLoopTask;
                } catch(OperationCanceledException) {
                    Console.Clear();
                }
            }
        }

        private IFeedDecoder CreateFeedDecoder(IServiceProvider serviceProvider)
        {
            var decoderOptions = new BaseStationFeedDecoderOptions();
            var decoderFactory = serviceProvider.GetRequiredService<FeedDecoderFactory>();
            var decoder = decoderFactory.Create(decoderOptions);

            return decoder;
        }

        private IReceiver OpenReceiver(IServiceProvider serviceProvider)
        {
            IReceiver result = null;

            if(_Options.ReceiverName != null) {
                Console.WriteLine($"Loading receiver {_Options.ReceiverName}");
                var receiverOptions = _ReceiverFactory.FindOptionsFor(_Options.ReceiverName.Trim());
                if(receiverOptions == null) {
                    OptionsParser.Usage($"Could not find receiver options for the \"{_Options.ReceiverName}\" receiver");
                } else if(receiverOptions.Connector == null || receiverOptions.FeedDecoder == null || receiverOptions.Enabled == false) {
                    Console.WriteLine($"Receiver {_Options.ReceiverName} cannot be used.");
                    if(receiverOptions.Connector == null) {
                        Console.WriteLine($"* The {nameof(receiverOptions.Connector)} settings cannot be parsed");
                    }
                    if(receiverOptions.FeedDecoder == null) {
                        Console.WriteLine($"* The {nameof(receiverOptions.FeedDecoder)} settings cannot be parsed");
                    }
                    if(!receiverOptions.Enabled) {
                        Console.WriteLine($"* The receiver is not enabled");
                    }
                    OptionsParser.Usage($"A receiver cannot be built from the options for {_Options.ReceiverName}");
                } else {
                    result = _ReceiverFactory.Build(serviceProvider, receiverOptions);
                    if(result == null) {
                        OptionsParser.Usage($"\"{_Options.ReceiverName}\" receiver has good options but a receiver could not be built from them");
                    }
                }
            }

            return result;
        }

        private IReceiveConnector OpenConnector(IServiceProvider serviceProvider)
        {
            return String.IsNullOrEmpty(_Options.FileName)
                ? OpenNetworkConnector(serviceProvider)
                : OpenRecordingConnector(serviceProvider);
        }

        private IReceiveConnector OpenNetworkConnector(IServiceProvider serviceProvider)
        {
            Console.WriteLine($"Connecting to BaseStation feed on {_Options.Address}:{_Options.Port}");
            if(!IPAddress.TryParse(_Options.Address, out var address)) {
                OptionsParser.Usage($"{_Options.Address} is not a valid IP address");
            }

            var connectorOptions = new TcpPullConnectorSettings() {
                Address =   address.ToString(),
                Port =      _Options.Port,
            };
            var connectorFactory = serviceProvider.GetRequiredService<ReceiveConnectorFactory>();
            var connector = connectorFactory.Create(connectorOptions);

            return connector;
        }

        private IReceiveConnector OpenRecordingConnector(IServiceProvider serviceProvider)
        {
            Console.WriteLine($"Replaying feed recording from {_Options.FileName}");

            var connectorOptions = new RecordingPlaybackConnectorOptions() {
                RecordingFileName = _Options.FileName,
                PlaybackSpeed =     _Options.PlaybackSpeed,
            };
            var connectorFactory = serviceProvider.GetRequiredService<ReceiveConnectorFactory>();
            var connector = connectorFactory.Create(connectorOptions);

            return connector;
        }
    }
}
