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
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using VirtualRadar.CommandLine;
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
    public class Command_ShowTerminal(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        IServiceProvider                _ServiceProvider,
        IAircraftOnlineLookupService    _AircraftLookupService,
        IReceiverFactory                _ReceiverFactory
        #pragma warning restore IDE1006
    ) : CommonCommand
    {
        public IPAddress Address { get; set; } = IPAddress.None;

        public int Port { get; set; }

        public FileInfo? RecordingFileInfo { get; set; }

        public double PlaybackSpeed { get; set; }

        public string? ReceiverName { get; set; }

        public async Task<bool> RunAsync()
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
                            aircraftList?.ApplyLookup(batchOutcome);
                        };
                    }

                    var aircraftListWindow = scope.ServiceProvider.GetRequiredService<AircraftListWindow>();
                    aircraftListWindow.AircraftList = receiver?.AircraftList ?? aircraftList;

                    var windowEventLoopTask = aircraftListWindow.EventLoop(cancelSource);

                    var receiverOrConnector = receiver?.Connector ?? connector;
                    if(receiverOrConnector == null) {
                        throw new InvalidOperationException("Could not start a connector");
                    }

                    receiverOrConnector.ConnectionStateChanged += (_,_) => aircraftListWindow.ConnectionState = receiverOrConnector.ConnectionState.ToString();

                    receiverOrConnector.LastExceptionChanged += (_,_) => aircraftListWindow.LastConnectorException = receiverOrConnector.LastException;

                    receiverOrConnector.PacketReceived += (_, packet) => {
                        ++aircraftListWindow.CountPacketsSeen;
                        if(receiver == null) {
                            feedDecoder?.ParseFeedPacket(packet);
                        }
                    };

                    if(receiver == null && feedDecoder != null) {
                        feedDecoder.MessageReceived += (_, message) => {
                            if(aircraftList != null) {
                                var applyOutcome = aircraftList.ApplyMessage(message);
                                if(applyOutcome.AddedAircraft && message.Icao24 != null) {
                                    _AircraftLookupService.Lookup(message.Icao24.Value);
                                }
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

            return true;
        }

        private IFeedDecoder? CreateFeedDecoder(IServiceProvider serviceProvider)
        {
            var decoderSettingsDto = new BaseStationFeedDecoderSettingsDto();
            var decoderFactory = serviceProvider.GetRequiredService<FeedDecoderFactory>();
            var decoder = decoderFactory.Create(decoderSettingsDto);

            return decoder;
        }

        private IReceiver? OpenReceiver(IServiceProvider serviceProvider)
        {
            IReceiver? result = null;

            if(ReceiverName != null) {
                Console.WriteLine($"Loading receiver {ReceiverName}");
                var receiverSettingsDto = _ReceiverFactory.FindSettingsDtoFor(ReceiverName.Trim());
                if(receiverSettingsDto == null) {
                    throw new BadParameterException(Commands.ShowTerminal, $"Could not find receiver options for the \"{ReceiverName}\" receiver");
                } else if(receiverSettingsDto.Connector == null || receiverSettingsDto.FeedDecoder == null || receiverSettingsDto.Enabled == false) {
                    Console.WriteLine($"Receiver {ReceiverName} cannot be used.");
                    if(receiverSettingsDto.Connector == null) {
                        Console.WriteLine($"* The {nameof(receiverSettingsDto.Connector)} settings cannot be parsed");
                    }
                    if(receiverSettingsDto.FeedDecoder == null) {
                        Console.WriteLine($"* The {nameof(receiverSettingsDto.FeedDecoder)} settings cannot be parsed");
                    }
                    if(!receiverSettingsDto.Enabled) {
                        Console.WriteLine($"* The receiver is not enabled");
                    }
                    throw new BadParameterException(Commands.ShowTerminal, $"A receiver cannot be built from the options for {ReceiverName}");
                } else {
                    result = _ReceiverFactory.Build(serviceProvider, receiverSettingsDto);
                    if(result == null) {
                        throw new BadParameterException(Commands.ShowTerminal, $"\"{ReceiverName}\" receiver has good options but a receiver could not be built from them");
                    }
                }
            }

            return result;
        }

        private IReceiveConnector OpenConnector(IServiceProvider serviceProvider)
        {
            return RecordingFileInfo == null
                ? OpenNetworkConnector(serviceProvider)
                : OpenRecordingConnector(serviceProvider);
        }

        private IReceiveConnector OpenNetworkConnector(IServiceProvider serviceProvider)
        {
            Console.WriteLine($"Connecting to BaseStation feed on {Address}:{Port}");

            var connectorSettingsDto = new TcpPullConnectorSettingsDto() {
                Address =   Address.ToString(),
                Port =      Port,
            };
            var connectorFactory = serviceProvider.GetRequiredService<ReceiveConnectorFactory>();
            var connector = connectorFactory.Create(connectorSettingsDto)
                ?? throw new InvalidOperationException($"Could not start network connector to {connectorSettingsDto}");

            return connector;
        }

        private IReceiveConnector OpenRecordingConnector(IServiceProvider serviceProvider)
        {
            Console.WriteLine($"Replaying feed recording from {RecordingFileInfo}");

            var connectorSettingsDto = new RecordingPlaybackConnectorSettingsDto() {
                RecordingFileName = RecordingFileInfo!.FullName,
                PlaybackSpeed =     PlaybackSpeed,
            };
            var connectorFactory = serviceProvider.GetRequiredService<ReceiveConnectorFactory>();
            var connector = connectorFactory.Create(connectorSettingsDto)
                ?? throw new InvalidOperationException($"Could not start playback connector to {connectorSettingsDto}");

            return connector;
        }
    }
}
