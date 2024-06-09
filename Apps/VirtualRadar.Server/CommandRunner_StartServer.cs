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
using System.Reflection;
using BlazorStrap;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VirtualRadar.Server
{
    class CommandRunner_StartServer : CommandRunner
    {
        public override async Task<bool> Run()
        {
            // Trying to get the .NET 8 web application to path from the application folder
            // instead of CWD is a pain in the backside... so I'm just going with the flow
            var currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            try {
                var builder = WebApplication.CreateBuilder();

                AddVirtualRadarServerGroups(builder.Services);

                builder.Services.AddMvc(options => {
                    options.EnableEndpointRouting = false;          // <-- need this for web API
                });
                builder.Services.AddControllers();                  // <-- need this for web API
                builder.Services
                    .AddRazorComponents()
                    .AddInteractiveServerComponents();

                builder.Services.AddBlazorStrap();

                if(!Options.ShowLog) {
                    builder.Logging.ClearProviders();
                }

                builder.WebHost.ConfigureKestrel((context, options) => {
                    Console.WriteLine($"Listening on http://localhost:{Options.HttpPort}");
                    options.ListenLocalhost(Options.HttpPort);
                
                    Console.WriteLine($"Listening on https://localhost:{Options.HttpsPort}");
                    options.ListenLocalhost(Options.HttpsPort, listenOptions => {
                        listenOptions.UseHttps();               // TODO: Need a *bunch* more stuff here
                    });
                });

                var app = builder.Build();

                if(!app.Environment.IsDevelopment()) {
                    app.UseExceptionHandler("/Error");
                }

                app.UseStaticFiles();
                app.UseRouting();
                app.UseMvcWithDefaultRoute();                       // <-- need this for web API
                app.UseAntiforgery();
                app.MapRazorComponents<VirtualRadar.Server.Components.App>()        // <-- VS2022 intellisense thinks this namespace does not exist, it will remove it and then fail to compile if you have it as a using and then CTRL+R+G
                   .AddInteractiveServerRenderMode();

                var cancellationSource = new CancellationTokenSource();

                await WriteLine("Booting VRS");
                BootVirtualRadarServer(app);

                Console.WriteLine($"Starting server");
                var serverTask = app.StartAsync(cancellationSource.Token);

                if(!Options.SuppressBrowser) {
                    var url = $"http://localhost:{Options.HttpPort}";
                    try {
                        await WriteLine($"Opening {url} in default browser");
                        ProcessStarter.OpenUrlInDefaultBrowser(url);
                    } catch(Exception ex) {
                        await WriteLine($"Could not open {url}: {ex.Message}");
                    }
                }

                Console.TreatControlCAsInput = true;
                await WriteLine("Press Q to shut down");
                var waitForKeyTask = CancelIfKeyPressed(cancellationSource, ConsoleKey.Q);

                await serverTask;

                if(cancellationSource.IsCancellationRequested) {
                    await WriteLine($"Shutting down");
                }

                await waitForKeyTask;
                cancellationSource.Cancel();

                return true;
            } finally {
                Environment.CurrentDirectory = currentDirectory;
            }
        }
    }
}
