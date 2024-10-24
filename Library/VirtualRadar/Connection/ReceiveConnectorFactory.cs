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
using VirtualRadar.Extensions;

namespace VirtualRadar.Connection
{
    [Lifetime(Lifetime.Transient)]
    public class ReceiveConnectorFactory(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        IServiceProvider _ServiceProvider
        #pragma warning restore IDE1006
    ) : IDisposable
    {
        private readonly object _SyncLock = new();

        public IReceiveConnector Connector { get; private set; }

        ~ReceiveConnectorFactory() => Dispose(false);

        public IReceiveConnector Create(IReceiveConnectorOptions options)
        {
            lock(_SyncLock) {
                if(options != null && Connector == null) {
                    var receiverType = ReceiveConnectorConfig.ReceiveConnectorType(options.GetType());
                    if(receiverType != null) {
                        Connector = (IReceiveConnector)ActivatorUtilities.CreateInstance(_ServiceProvider, receiverType, options);
                    }
                }
            }

            return Connector;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                var connector = Connector;
                Task.Run(() => connector?.DisposeAsync());
            }
        }
    }
}
