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
    /// Maintains a list of callback handles and disposes of them when it itself disposed. A small
    /// quality-of-life thing for classes that expect to be registering and disposing of more than
    /// one callback. This class is typesafe.
    /// </summary>
    public class CallbackHandleDisposer : IDisposable
    {
        private readonly object _SyncLock = new();
        private bool _Disposed;

        private readonly List<ICallbackHandle> _CallbackHandles = [];
        /// <summary>
        /// The list of registered callback handles.
        /// </summary>
        public ICallbackHandle[] CallbackHandles
        {
            get {
                lock(_SyncLock) {
                    return _CallbackHandles.ToArray();
                }
            }
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~CallbackHandleDisposer() => Dispose(false);

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
                Clear();
            }
        }

        /// <summary>
        /// Records an instance of a callback handle that will be disposed when the object is disposed.
        /// Throws an exception if Dispose has previously been called on this object - it would be
        /// dangerous to do nothing because the handle will leak if the caller is relying on us to dispose
        /// of it.
        /// </summary>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        public CallbackHandle<TArgs> Add<TArgs>(CallbackHandle<TArgs> handle)
        {
            ArgumentNullException.ThrowIfNull(handle);

            lock(_SyncLock) {
                if(_Disposed) {
                    throw new ObjectDisposedException(
                        nameof(handle),
                        $"Cannot add {handle.GetType().Name} handles to disposed instances of {nameof(CallbackHandleDisposer)}"
                    );
                }
                if(!_CallbackHandles.Contains(handle)) {
                    _CallbackHandles.Add(handle);
                }
            }
            return handle;
        }

        /// <summary>
        /// Records the instances of multiple callback handles that will be disposed when the object is disposed.
        /// Throws an exception if Dispose has previously been called on this object - it would be
        /// dangerous to do nothing because the handle will leak if the caller is relying on us to dispose
        /// of it.
        /// </summary>
        /// <param name="callbackHandles"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void AddMany(params ICallbackHandle[] callbackHandles)
        {
            ArgumentNullException.ThrowIfNull(callbackHandles);

            lock(_SyncLock) {
                if(_Disposed) {
                    throw new ObjectDisposedException(
                        nameof(callbackHandles),
                        $"Cannot add {String.Join(", ", callbackHandles.Select(ch => ch.GetType().Name))} " +
                        $"handles to disposed instances of {nameof(CallbackHandleDisposer)}"
                    );
                }
                foreach(var handle in callbackHandles) {
                    if(!_CallbackHandles.Contains(handle)) {
                        _CallbackHandles.Add(handle);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a specific callback handle.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool Remove(ICallbackHandle handle)
        {
            var result = handle != null;

            if(result) {
                lock(_SyncLock) {
                    result = _CallbackHandles.Remove(handle);
                    if(result) {
                        handle.Dispose();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Releases all callback handles early.
        /// </summary>
        public void Clear()
        {
            lock(_SyncLock) {
                while(_CallbackHandles.Count > 0) {
                    Remove(_CallbackHandles[0]);
                }
            }
        }
    }
}
