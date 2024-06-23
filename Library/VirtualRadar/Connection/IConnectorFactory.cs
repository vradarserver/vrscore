// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Connection
{
    /// <summary>
    /// Builds connectors from options objects.
    /// </summary>
    [Lifetime(Lifetime.Singleton)]
    public interface IConnectorFactory
    {
        /// <summary>
        /// Registers the connector that can be built from an options object. Only one type of connector can
        /// be associated with any given type of options. Second and subsequent calls for a given type of
        /// option will overwrite the registration for previous types.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <typeparam name="TConnector"></typeparam>
        /// <returns></returns>
        void RegisterConnectorByOptions<TOptions, TConnector>()
            where TConnector : IConnector, IOneTimeConfigurable<TOptions>;

        /// <summary>
        /// Builds a new connector from the options passed across.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ConnectorNotRegisteredException"></exception>
        IConnector Build(object options);

        /// <summary>
        /// Builds a new connector from the options passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        T Build<T>(object options) where T: IConnector;
    }
}
