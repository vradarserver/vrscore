// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Collections;

namespace VirtualRadar
{
    /// <summary>
    /// Describes a collection of Task delegates that can be passed and work upon an immutable object in
    /// parallel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Similar to a bog-standard event handler except the handlers are called in parallel and the args are
    /// not meant to be mutable. That 'not mutable' bit is not policed by this, the args could be anything,
    /// but things will get freaky if the handlers are allowed to change the args.
    /// </remarks>
    public class ParallelEvent<TArgs>
    {
        private object _SyncLock = new();

        private volatile List<HandlerDelegate> _Handlers = new();

        /// <summary>
        /// The delegate for handlers in the pipeline.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate Task HandlerDelegate(object sender, TArgs args);

        /// <summary>
        /// Adds a new handler. If the handler is already present then false is returned and the handler is
        /// not attached.
        /// </summary>
        /// <param name="handler"></param>
        public bool AddHandler(HandlerDelegate handler)
        {
            var handlers = _Handlers;
            var result = !handlers.Contains(handler);
            if(result) {
                lock(_SyncLock) {
                    handlers = ShallowCollectionCopier.Copy(_Handlers);
                    result = !handlers.Contains(handler);
                    if(result) {
                        handlers.Add(handler);
                        _Handlers = handlers;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Removes an existing handler. If the handler is not present then false is returned.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public bool RemoveHandler(HandlerDelegate handler)
        {
            var handlers = _Handlers;
            var result = handlers.Contains(handler);
            if(result) {
                lock(_SyncLock) {
                    handlers = ShallowCollectionCopier.Copy(_Handlers);
                    result = handlers.Remove(handler);
                    if(result) {
                        _Handlers = handlers;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sends args to all handlers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="immutableArgs"></param>
        /// <returns></returns>
        public async Task Send(object sender, TArgs immutableArgs)
        {
            var handlers = _Handlers;
            if(handlers.Count > 0) {
                var tasks = new Task[handlers.Count];
                for(var idx = 0;idx < handlers.Count;++idx) {
                    tasks[idx] = handlers[idx](sender, immutableArgs);
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
