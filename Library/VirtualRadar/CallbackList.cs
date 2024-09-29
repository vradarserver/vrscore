// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar
{
    /// <summary>
    /// A threadsafe collection of <see cref="CallbackHandle"/> objects and methods to call them sequentially.
    /// </summary>
    /// <typeparam name="TArgs"></typeparam>
    public class CallbackList<TArgs> : IDisposable
    {
        private readonly object _SyncLock = new();
        private bool _Disposed;
        private readonly List<CallbackHandle<TArgs>> _CallbackHandles = [];

        // Finaliser
        ~CallbackList() => Dispose(false);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _Disposed = true;
                lock(_SyncLock) {
                    foreach(var callback in _CallbackHandles) {
                        callback.ReleaseOwner();
                    }
                    _CallbackHandles.Clear();
                }
            }
        }

        /// <summary>
        /// Adds a callback function and returns a handle to it. The handle must be disposed to release
        /// the callback. If it is not disposed then the caller's lifetime becomes tied to the callback
        /// list's lifetime.
        /// </summary>
        /// <param name="callback">The delegate to call.</param>
        /// <returns></returns>
        public CallbackHandle<TArgs> Add(Action<TArgs> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            if(_Disposed) {
                throw new ObjectDisposedException(
                    nameof(callback),
                    $"Cannot add a callback to a disposed instance of {GetType().Name}"
                );
            }

            lock(_SyncLock) {
                var result = new CallbackHandle<TArgs>(this, callback);
                _CallbackHandles.Add(result);

                return result;
            }
        }

        /// <summary>
        /// Removes the callback passed across.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        internal bool Remove(CallbackHandle<TArgs> handle)
        {
            lock(_SyncLock) {
                var result = _CallbackHandles.Remove(handle);
                if(result) {
                    handle.ReleaseOwner();
                }
                return result;
            }
        }

        /// <summary>
        /// Invokes all callbacks in the order that they were added. Note that two simultaneous calls to this
        /// from two different threads will not block each other. This class is thread-safe and guarantees
        /// sequential calls to the callbacks within each invocation of <see cref="Invoke"/>, but if the function
        /// is called from multiple threads then you will get simultaneous invocations on each thread.
        /// </summary>
        /// <param name="args"></param>
        public void Invoke(TArgs args)
        {
            CallbackHandle<TArgs>[] callbacks;
            lock(_SyncLock) {
                callbacks = _CallbackHandles.ToArray();
            }

            foreach(var callback in callbacks) {
                callback.Invoke(args);
            }
        }

        /// <summary>
        /// See <see cref="Invoke"/>, except this invokes all callbacks regardless of whether any of them throws
        /// an exception. Any exceptions thrown are wrapped in an aggregate exception which is returned to the
        /// caller to deal with.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="aggregateExceptionMessage"></param>
        /// <returns></returns>
        public AggregateException InvokeWithoutExceptions(TArgs args, string aggregateExceptionMessage = "Exceptions were encountered while invoking callbacks")
        {
            CallbackHandle<TArgs>[] callbacks;
            lock(_SyncLock) {
                callbacks = _CallbackHandles.ToArray();
            }

            List<Exception> exceptions = null;
            foreach(var callback in callbacks) {
                try {
                    callback.Invoke(args);
                } catch(Exception ex) {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }

            return exceptions == null
                ? null
                : new AggregateException(aggregateExceptionMessage, exceptions);
        }
    }
}
