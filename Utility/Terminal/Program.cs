﻿// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtualRadar.Configuration;
using VirtualRadar.Feed.BaseStation;
using VirtualRadar.Feed.Recording;

namespace VirtualRadar.Utility.Terminal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var exitCode = 0;
            Options options = null;

            Console.OutputEncoding = Encoding.UTF8;

            try {
                options = OptionsParser.Parse(args);

                var builder = Host.CreateDefaultBuilder();
                builder.ConfigureServices((context, services) => {
                    services
                        .Configure<AircraftOnlineLookupServiceOptions>(opt => {
                            opt.ExpireQueueAfterMinutes = 1;
                        })

                        .AddVirtualRadarGroup()
                        .AddBaseStationFeedGroup()
                        .AddFeedRecordingGroup()

                        .AddSingleton<Options>(options)
                        .AddScoped<AircraftListWindow, AircraftListWindow>()

                        // This will do for now, I just want to see it working (or not)...
                        .AddScoped<TempRunner, TempRunner>()
                    ;
                });

                using(var host = builder.Build()) {
                    using(var scope = host.Services.CreateScope()) {
                        var bootService = scope.ServiceProvider.GetRequiredService<BootService>();
                        bootService.Start();

                        var tempRunner = scope.ServiceProvider.GetRequiredService<TempRunner>();
                        await tempRunner.Run();
                    }
                }
            } catch(Exception ex) {
                Console.WriteLine($"Caught exception during processing: {ex}");
                exitCode = 2;
            }

            Environment.Exit(exitCode);
        }
    }
}
