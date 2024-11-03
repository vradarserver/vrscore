// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Receivers
{
    /// <summary>
    /// Manages receiver message sources for the program.
    /// </summary>
    [Lifetime(Lifetime.Singleton)]
    public interface IReceiverFactory
    {
        /// <summary>
        /// Returns all active receivers.
        /// </summary>
        IReceiver[] Receivers { get; }

        /// <summary>
        /// Returns the current options for the receiver name passed across.
        /// </summary>
        /// <param name="receiverName"></param>
        /// <returns></returns>
        ReceiverOptions FindOptionsFor(string receiverName);

        /// <summary>
        /// Creates a new receiver using the options passed across. The receiver's lifetime is not managed by
        /// the factory, and if the factory already has a receiver with the same options then it is not
        /// reused. Callbacks are not called when this function creates a receiver, nor are they called if/when
        /// the receiver is disposed. Will return null if the options are invalid.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        IReceiver Build(IServiceProvider serviceProvider, ReceiverOptions options);

        /// <summary>
        /// Returns a receiver previously created by <see cref="FindOrBuild"/> with the name passed across.
        /// </summary>
        /// <param name="receiverName"></param>
        /// <returns></returns>
        IReceiver FindByName(string receiverName);

        /// <summary>
        /// Returns a receiver previously created by <see cref="FindOrBuild"/> with the ID passed across.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IReceiver FindById(int id);

        /// <summary>
        /// Returns a pre-built receiver that matches the configured default source. If there is no default
        /// source, or if it cannot be found, then the first receiver is returned. If there are no receivers
        /// then null is returned.
        /// </summary>
        /// <returns></returns>
        IReceiver FindDefaultSource();

        /// <summary>
        /// Registers a callback that is called whenever a receiver is added. The caller should keep the handle
        /// returned by this function and dispose of it when they themselves are disposed, otherwise they will
        /// tie themselves to the lifetime of this factory.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        ICallbackHandle ReceiverAddedCallback(Action<IReceiver> callback);

        /// <summary>
        /// Registers a callback that is called whenever a receiver is about to be disposed. The caller should
        /// keep the handle returned by this function and dispose of it when they themselves are disposed,
        /// otherwise they will tie themselves to the lifetime of this factory.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        ICallbackHandle ReceiverShuttingDownCallback(Action<IReceiver> callback);

        /// <summary>
        /// Finds an existing receiver that has the same name (case insensitive) as the options passed across.
        /// If no such receiver exists then a new receiver is created and returned. If the receiver exists then
        /// its options are compared. If the options are unchanged then the existing receiver is returned,
        /// otherwise the existing receiver is disposed and a new receiver returned. In all cases the factory
        /// manages the lifetime of the receiver, it should not be disposed by the caller.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        (bool Added, IReceiver Receiver) FindOrBuild(ReceiverOptions options);

        /// <summary>
        /// Shuts a receiver down.
        /// </summary>
        /// <param name="receiver"></param>
        void ShutDownReceiver(IReceiver receiver);
    }
}
