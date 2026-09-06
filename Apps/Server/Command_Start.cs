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
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VirtualRadar.CommandLine;
using VirtualRadar.Configuration;
using VirtualRadar.Server.Middleware;

namespace VirtualRadar.Server
{
    public class Command_Start : CommonCommand
    {
        public bool Dev { get; set; }

        public string WorkingFolder { get; set; } = Defaults.DefaultWorkingFolder;

        public int HttpPort { get; set; }

        public int HttpsPort { get; set; }

        public bool NoBrowser { get; set; }

        public bool NoHttp { get; set; }

        public bool NoHttps { get; set; }

        public bool ShowLog { get; set; }

        public async Task<bool> RunAsync()
        {
            if(Dev) {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            }

            var version = InformationalVersion.VirtualRadarVersion;

            var title = $"Virtual Radar Server Core {version}";
            await WriteLineAsync(title);
            await WriteLineAsync(new String('=', title.Length));

            // Trying to get the .NET 8 web application to path from the application folder
            // instead of CWD is a pain in the backside... so I'm just going with the flow
            var currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
                ?? throw new InvalidOperationException($"{Assembly.GetEntryAssembly()?.FullName ?? "Null"} entry assembly has no {nameof(Assembly.Location)}");

            try {
                var builder = WebApplication.CreateBuilder();

                builder.Services.AddVirtualRadarServer();
                var vrsWorkingFolder = WorkingFolder;
                await WriteLineAsync($"Working folder is {vrsWorkingFolder}");

                builder.Services.AddMvc(options => {
                    options.EnableEndpointRouting = false;          // <-- need this for web API
                });
                builder
                    .Services
                    .AddControllers()                               // <-- need this for web API
                    .AddNewtonsoftJson(config => {
                        // The .NET JSON serialiser is off-limits
                        config.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver() {
                            NamingStrategy = new Newtonsoft.Json.Serialization.DefaultNamingStrategy(),
                        };
                    });
                builder.Services
                    .AddRazorComponents()
                    .AddInteractiveServerComponents();

                builder.Services.AddBlazorStrap();

                if(!ShowLog) {
                    builder.Logging.ClearProviders();
                }

                var w3cLoggingEnabled = ConfigureW3CLogging(builder, vrsWorkingFolder);
                ConfigureKestrel(builder);

                var app = builder.Build();
                await WriteLineAsync($"Environment is {app.Environment.EnvironmentName}");

                if(!app.Environment.IsDevelopment()) {
                    app.UseExceptionHandler("/Error");
                }

                if(w3cLoggingEnabled) {
                    app.UseW3CLogging();
                }

                app.UseV3StaticFileMiddleware();
                app.UseStaticFiles();

                app.UseRouting();
                app.UseMvcWithDefaultRoute();                       // <-- need this for web API
                app.UseAntiforgery();
                app.MapRazorComponents<VirtualRadar.Server.Components.App>()        // <-- VS2022 intellisense thinks this namespace does not exist, it will remove it and then fail to compile if you have it as a using and then CTRL+R+G
                   .AddInteractiveServerRenderMode();

                var serverCancel = new CancellationTokenSource();

                await WriteLineAsync("Booting VRS");
                app.StartVirtualRadarServer(config => {
                    config.WorkingFolder = vrsWorkingFolder;
                });

                try {
                    Console.WriteLine($"Starting server");
                    var serverTask = app.StartAsync(serverCancel.Token);

                    if(!NoBrowser) {
                        var url = $"http://localhost:{HttpPort}/admin";
                        try {
                            await WriteLineAsync($"Opening {url} in default browser");
                            ProcessStarter.OpenUrlInDefaultBrowser(url);
                        } catch(Exception ex) {
                            await WriteLineAsync($"Could not open {url}: {ex.Message}");
                        }
                    }

                    Console.TreatControlCAsInput = true;
                    await WriteLineAsync("Press Q to shut down cleanly");
                    var waitForKeyTask = CancelOnKeyPress.IfKeyPressed(serverCancel, ConsoleKey.Q);

                    await serverTask;

                    if(serverCancel.IsCancellationRequested) {
                        await WriteLineAsync($"Shutting down");
                    }

                    await waitForKeyTask;
                    serverCancel.Cancel();
                } finally {
                    app.StopVirtualRadarServer();
                }

                return true;
            } finally {
                Environment.CurrentDirectory = currentDirectory;
            }
        }

        private static bool ConfigureW3CLogging(WebApplicationBuilder builder, string vrsWorkingFolder)
        {
            const string w3cLoggingSection = "W3CLogging";
            var w3cLoggingEnabled = builder.Configuration.GetValue<bool>($"{w3cLoggingSection}:Enabled");

            if(w3cLoggingEnabled) {
                const string logDirectoryName = nameof(W3CLoggerOptions.LogDirectory);

                var section = builder.Configuration.GetSection(w3cLoggingSection);
                if(String.IsNullOrEmpty(section[logDirectoryName])) {
                    var logDirectory = Path.Combine(vrsWorkingFolder, "W3CLog");
                    if(!Directory.Exists(logDirectory)) {
                        Directory.CreateDirectory(logDirectory);
                    }
                    section[logDirectoryName] = logDirectory;
                }
                Console.WriteLine($"W3C log folder is {section[logDirectoryName]}");

                builder.Services.AddW3CLogging(config => section.Bind(config));
            }

            return w3cLoggingEnabled;
        }

        private void ConfigureKestrel(WebApplicationBuilder builder)
        {
            builder.WebHost.ConfigureKestrel((context, options) => {
                if(!NoHttp) {
                    Console.WriteLine($"Listening on http://localhost:{HttpPort}");
                    options.ListenLocalhost(HttpPort);
                }

                if(!NoHttps) {
                    Console.WriteLine($"Listening on https://localhost:{HttpsPort}");
                    options.ListenLocalhost(HttpsPort, listenOptions => {
                        listenOptions.UseHttps();               // TODO: Need a *bunch* more stuff here
                    });
                }
            });
        }
    }
}
