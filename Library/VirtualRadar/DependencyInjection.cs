// Copyright © 2022 onwards, Andrew Whewell
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
using VirtualRadar.Extensions;
using VirtualRadar.Reflection;

namespace VirtualRadar
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Loads all of the Virtual Radar modules and plugins and then calls their RegisterServices
        /// implementation in order of priority.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddVirtualRadarServer(this IServiceCollection services)
        {
            VirtualRadarModuleFactory.DiscoverModules();
            VirtualRadarModuleFactory.CallLoadedModules(
                module => module.ModuleInstance.RegisterServices(services),
                ignoreExceptions: false
            );

            return services;
        }

        /// <summary>
        /// Starts up Virtual Radar Server's background services. Always pair this with a call to <see
        /// cref="StopVirtualRadarServer"/>.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IHost StartVirtualRadarServer(this IHost host)
        {
            var log = host.Services.GetRequiredService<ILog>();
            log.Message($"Virtual Radar Server Core {InformationalVersion.VirtualRadarVersion} starting up.");

            VirtualRadarModuleFactory.CallLoadedModules(
                module => host.Services.InjectServices(module.ModuleInstance),
                ignoreExceptions: false
            );

            VirtualRadarModuleFactory.CallLoadedModules(
                module => module.ModuleInstance.Start(),
                ignoreExceptions: false
            );

            log.Message($"Virtual Radar Server Core started.");

            return host;
        }

        /// <summary>
        /// Stops Virtual Radar Server's background services. Do not call this unless <see
        /// cref="StartVirtualRadarServer"/> has been called previously.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IHost StopVirtualRadarServer(this IHost host)
        {
            var log = host.Services.GetRequiredService<ILog>();
            log.Message($"Virtual Radar Server Core stopping.");

            try {
                VirtualRadarModuleFactory.CallLoadedModules(
                    module => module.ModuleInstance.Stop(),
                    ignoreExceptions: true,
                    inReverseOrder: true
                );
            } finally {
                log.Message($"Virtual Radar Server Core stopped.");
            }

            return host;
        }

        /// <summary>
        /// Adds the VirtualRadar.dll services. Do not call this if you have called <see cref="AddVirtualRadarServer"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddVirtualRadarGroup(this IServiceCollection services)
        {
            services.AddLifetime<IAircraftList,                 AircraftList>();
            services.AddLifetime<IAircraftOnlineLookupProvider, Services.AircraftOnlineLookup.LookupProvider>();
            services.AddLifetime<IAircraftOnlineLookupService,  Services.AircraftOnlineLookup.LookupService>();
            services.AddLifetime<IClock,                        Services.Clock>();
            services.AddLifetime<IFileSystem,                   Services.FileSystem>();
            services.AddLifetime<IHttpClientService,            Services.HttpClientService>();
            services.AddLifetime<ILog,                          Services.LogService>();
            services.AddLifetime<IModuleInformationService,     Services.ModuleInformationService>();
            services.AddLifetime<IWebAddressManager,            Services.WebAddressManager>();
            services.AddLifetime<IWorkingFolder,                Services.WorkingFolder>();

            services.AddLifetime<Configuration.ISettingsStorage,        Configuration.SettingsStorage>();
            services.AddLifetime<Configuration.ISettingsConfiguration,  Configuration.SettingsConfiguration>();

            services.AddLifetime<Connection.ReceiveConnectorFactory, Connection.ReceiveConnectorFactory>();

            services.AddLifetime<Feed.FeedDecoderFactory,           Feed.FeedDecoderFactory>();
            services.AddLifetime<Feed.IFeedFormatFactoryService,    Feed.FeedFormatFactoryService>();

            services.AddLifetime<Receivers.IReceiverFactory, Receivers.ReceiverFactory>();
            services.AddLifetime<Receivers.ReceiverEngine>();

            services.AddLifetime<StandingData.IRegistrationPrefixLookup,        StandingData.RegistrationPrefixLookup>();
            services.AddLifetime<StandingData.IStandingDataManager,             StandingData.StandingDataManager>();
            services.AddLifetime<StandingData.IStandingDataOverridesRepository, StandingData.StandingDataOverridesRepository>();
            services.AddLifetime<StandingData.IStandingDataUpdater,             StandingData.StandingDataUpdater>();

            services.AddLifetime<WebSite.IAircraftListJsonBuilder, WebSite.AircraftListJsonBuilder>();

            Configuration.ConfigurationConfig.RegisterAssembly(services);
            Connection.ReceiveConnectorConfig.RegisterAssembly();

            return services;
        }
    }
}
