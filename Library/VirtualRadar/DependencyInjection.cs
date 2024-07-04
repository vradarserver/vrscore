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

namespace VirtualRadar
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddVirtualRadarGroup(this IServiceCollection services)
        {
            services.AddLifetime<BootService,                   BootService>();
            services.AddLifetime<IAircraftList,                 AircraftList>();
            services.AddLifetime<IAircraftOnlineLookupCache,    Services.AircraftOnlineLookup.Cache>();
            services.AddLifetime<IAircraftOnlineLookupProvider, Services.AircraftOnlineLookup.LookupProvider>();
            services.AddLifetime<IAircraftOnlineLookupService,  Services.AircraftOnlineLookup.LookupService>();
            services.AddLifetime<IFileSystem,                   Services.FileSystem>();
            services.AddLifetime<IHttpClientService,            Services.HttpClientService>();
            services.AddLifetime<IWebAddressManager,            Services.WebAddressManager>();
            services.AddLifetime<IWorkingFolder,                Services.WorkingFolder>();

            services.AddLifetime<Connection.IConnectorFactory,  Connection.ConnectorFactory>();
            services.AddLifetime<Connection.TcpPullConnector,   Connection.TcpPullConnector>();

            services.AddLifetime<Feed.IFeedDecoderFactory,          Feed.FeedDecoderFactory>();
            services.AddLifetime<Feed.IFeedFormatFactoryService,    Feed.FeedFormatFactoryService>();

            services.AddLifetime<StandingData.IStandingDataManager,             StandingData.StandingDataManager>();
            services.AddLifetime<StandingData.IStandingDataOverridesRepository, StandingData.StandingDataOverridesRepository>();
            services.AddLifetime<StandingData.IStandingDataUpdater,             StandingData.StandingDataUpdater>();

            return services;
        }
    }
}
