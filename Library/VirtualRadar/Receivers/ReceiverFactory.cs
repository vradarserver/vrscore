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
using VirtualRadar.Collections;
using VirtualRadar.Configuration;
using VirtualRadar.Connection;
using VirtualRadar.Feed;

namespace VirtualRadar.Receivers
{
    /// <summary>
    /// Default implementation of <see cref="IReceiverFactory"/>.
    /// </summary>
    /// <param name="_ServiceProvider"></param>
    /// <param name="_Settings"></param>
    /// <param name="_Log"></param>
    class ReceiverFactory(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        IServiceProvider _ServiceProvider,
        ISettingsStorage _Settings,
        ILog _Log
        #pragma warning restore IDE1006
    ) : IReceiverFactory, IDisposable
    {
        private readonly object _SyncLock = new();
        private bool _Disposed;
        private volatile List<Receiver> _Receivers = [];
        private readonly CallbackWithParamList<IReceiver> _ReceiverAddedCallbacks = new();
        private readonly CallbackWithParamList<IReceiver> _ReceiverShuttingDownCallbacks = new();

        /// <inheritdoc/>
        public IReceiver[] Receivers
        {
            get {
                var receivers = _Receivers;
                return receivers.ToArray();
            }
        }

        ~ReceiverFactory() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                lock(_SyncLock) {
                    if(!_Disposed) {
                        _Disposed = true;

                        var receivers = _Receivers.ToArray();
                        _Receivers.Clear();
                        foreach(var receiver in receivers) {
                            ShutDownReceiver(receiver);
                        }

                        _ReceiverAddedCallbacks.Dispose();
                        _ReceiverShuttingDownCallbacks.Dispose();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public ReceiverOptions FindOptionsFor(string receiverName)
        {
            var messageSources = _Settings.LatestValue<MessageSourcesOptions>();
            return messageSources
                .Receivers
                .FirstOrDefault(receiver => String.Equals(receiver.Name, receiverName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <inheritdoc/>
        public Receiver Build(IServiceProvider serviceProvider, ReceiverOptions options)
        {
            Receiver result = null;

            IReceiveConnector connector = null;
            if(options?.Connector != null) {
                var connectorFactory = serviceProvider.GetRequiredService<ReceiveConnectorFactory>();
                connector = connectorFactory.Create(options.Connector);
            }

            IFeedDecoder feedDecoder = null;
            if(options?.FeedDecoder != null) {
                var decoderFactory = serviceProvider.GetRequiredService<FeedDecoderFactory>();
                feedDecoder = decoderFactory.Create(options.FeedDecoder);
            }

            if(connector != null && feedDecoder != null) {
                result = new(
                    options,
                    connector,
                    feedDecoder,
                    serviceProvider.GetRequiredService<IAircraftOnlineLookupService>()
                );
            }

            return result;
        }

        /// <inheritdoc/>
        IReceiver IReceiverFactory.Build(IServiceProvider serviceProvider, ReceiverOptions options) => Build(serviceProvider, options);

        /// <inheritdoc/>
        public IReceiver FindByName(string receiverName)
        {
            var receivers = _Receivers;
            return receivers.FirstOrDefault(receiver => String.Equals(
                receiver.Name,
                receiverName,
                StringComparison.InvariantCultureIgnoreCase
            ));
        }

        /// <inheritdoc/>
        public IReceiver FindById(int id)
        {
            var receivers = _Receivers;
            return receivers.FirstOrDefault(receiver => receiver.Id == id);
        }

        /// <inheritdoc/>
        public IReceiver FindById(int id, bool ignoreInvisibleReceivers, bool fallbackToDefaultReceiver)
        {
            var result = FindById(id);

            if(result == null || (result.Hidden && ignoreInvisibleReceivers)) {
                result = FindDefaultSource();
                if((result?.Hidden ?? false) && ignoreInvisibleReceivers) {
                    result = null;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public IReceiver FindDefaultSource()
        {
            var messageSources = _Settings.LatestValue<MessageSourcesOptions>();
            var receivers = _Receivers;
            return FindById(messageSources.DefaultSourceId)
                ?? receivers
                    .Where(r => !r.Hidden)
                    .OrderBy(r => r.Id)
                    .FirstOrDefault();
        }

        /// <inheritdoc/>
        public ICallbackHandle ReceiverAddedCallback(Action<IReceiver> callback) => _ReceiverAddedCallbacks.Add(callback);

        /// <inheritdoc/>
        public ICallbackHandle ReceiverShuttingDownCallback(Action<IReceiver> callback) => _ReceiverShuttingDownCallbacks.Add(callback);

        /// <inheritdoc/>
        public (bool Added, IReceiver Receiver) FindOrBuild(ReceiverOptions options)
        {
            var added = false;
            Receiver receiver = null;

            if(options?.Enabled ?? false) {
                lock(_SyncLock) {
                    if(_Disposed) {
                        throw new InvalidOperationException($"Cannot build more receivers, {nameof(ReceiverFactory)} has been disposed");
                    }

                    receiver = FindById(options.Id) as Receiver;
                    if(receiver == null || !receiver.Options.Equals(options)) {
                        var newReceivers = ShallowCollectionCopier.Copy(_Receivers);
                        if(receiver != null) {
                            newReceivers.Remove(receiver);
                            ShutDownReceiver(receiver);
                        }

                        receiver = Build(_ServiceProvider, options);
                        if(receiver != null) {
                            newReceivers.Add(receiver);
                            added = true;
                            var aggregateException = _ReceiverAddedCallbacks.InvokeWithoutExceptions(receiver);
                            if(aggregateException != null) {
                                _Log.Exception(
                                    aggregateException,
                                    $"Exception(s) caught while notifying callbacks that a new instance of " +
                                    $"receiver \"{receiver.Name}\" has been created."
                                );
                            }
                        }

                        _Receivers = newReceivers;
                    }
                }
            }

            return (Added: added, Receiver: receiver);
        }

        /// <summary>
        /// Disposes of the receiver passed across, logging and swallowing any exceptions raised.
        /// </summary>
        /// <param name="receiverInterface"></param>
        public void ShutDownReceiver(IReceiver receiverInterface)
        {
            if(receiverInterface is Receiver receiver) {
                try {
                    lock(_SyncLock) {
                        _Receivers.Remove(receiver);
                    }

                    var aggregateException = _ReceiverShuttingDownCallbacks.InvokeWithoutExceptions(receiver);
                    if(aggregateException != null) {
                        _Log.Exception(
                            aggregateException,
                            $"Exception(s) caught while notifying callbacks that an instance of " +
                            $"receiver \"{receiver.Name}\" is about to be disposed."
                        );
                    }
                    receiver.Dispose();
                } catch(Exception ex) {
                    _Log.Exception(ex, $"Exception caught when disposing of the {receiver} {receiver.GetType().Name} receiver");
                }
            }
        }
    }
}
