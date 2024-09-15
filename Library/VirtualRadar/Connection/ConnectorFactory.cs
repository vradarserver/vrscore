// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Extensions.DependencyInjection;

namespace VirtualRadar.Connection
{
    /// <summary>
    /// Default implementation of the connector factory.
    /// </summary>
    class ConnectorFactory : IConnectorFactory
    {
        private readonly object _SyncLock = new();

        private IServiceProvider _ServiceProvider;

        private Dictionary<Type, Func<IConnectorOptions, IConnector>> _OptionsTypeToBuildFunctionMap = new();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ConnectorFactory(IServiceProvider serviceProvider)
        {
            _ServiceProvider = serviceProvider;
            RegisterBuiltInConnectors();
        }

        private void RegisterBuiltInConnectors()
        {
            RegisterConnectorByOptions<TcpPullConnectorSettings, TcpPullConnector>();
        }

        /// <inheritdoc/>
        public void RegisterConnectorByOptions<TOptions, TConnector>()
            where TOptions: IConnectorOptions
            where TConnector : IConnector, IOneTimeConfigurable<TOptions>
        {
            lock(_SyncLock) {
                _OptionsTypeToBuildFunctionMap[typeof(TOptions)] =
                    config => {
                        var connector = _ServiceProvider.GetRequiredService<TConnector>();
                        connector.Configure((TOptions)config);
                        return connector;
                    }
                ;
            }
        }

        /// <inheritdoc/>
        public IConnector Build(IConnectorOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            lock(_SyncLock) {
                if(!_OptionsTypeToBuildFunctionMap.TryGetValue(options.GetType(), out var buildFunction)) {
                    throw new ConnectorNotRegisteredException($"There is no connector associated with {options.GetType().Name} options");
                }
                return buildFunction(options);
            }
        }

        /// <inheritdoc/>
        public T Build<T>(IConnectorOptions options) where T: IConnector => (T)Build(options);
    }
}
