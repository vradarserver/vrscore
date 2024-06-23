// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Extensions;

namespace VirtualRadar
{
    /// <summary>
    /// Implements a simple state object that has a private cancellable token that
    /// gets linked to an external token and has a non-reentrant TearDown method.
    /// The TearDown method is triggered when the linked cancellation occurs.
    /// </summary>
    public class CancellableState
    {
        private bool _TearingDown;

        /// <summary>
        /// Cancelling this will cancel <see cref="LinkedCancelToken"/>.
        /// </summary>
        public CancellationTokenSource PrivateCancelToken { get; private set; }

        /// <summary>
        /// A linked token between <see cref="PrivateCancelToken"/> and whatever was passed
        /// into <see cref="SetupCancellation"/>.
        /// </summary>
        public CancellationTokenSource LinkedCancelToken { get; private set; }

        /// <summary>
        /// Creates <see cref="PrivateCancelToken"/> and then links that and <paramref name="userToken"/>
        /// into a single <see cref="LinkedCancelToken"/>. The <see cref="TearDown"/> function is
        /// then registered to <see cref="LinkedCancelToken"/>.
        /// </summary>
        /// <param name="userToken"></param>
        public void SetupCancellation(CancellationToken userToken)
        {
            PrivateCancelToken = new();
            LinkedCancelToken = CancellationTokenSource.CreateLinkedTokenSource(
                PrivateCancelToken.Token,
                userToken
            );
            LinkedCancelToken.Token.Register(TearDown);
        }

        /// <summary>
        /// Tears the object down. If tearing the object down triggers a re-enterant
        /// call to the teardown then the re-enterant call immediately returns without
        /// doing anything.
        /// </summary>
        public void TearDown()
        {
            // The assumption is that a simple re-enterancy guard is enough, we don't need to lock.
            // We can't really lock safely here, it would need more understanding of the context. If
            // you are here to add locking then maybe change the caller to use something else instead.
            if(!_TearingDown) {
                _TearingDown = true;

                try {
                    TearDownState();
                    TearDownCancellationTokens(null);
                } finally {
                    _TearingDown = false;
                }
            }
        }

        protected void CancelEverything(List<Exception> exceptions)
        {
            if(PrivateCancelToken != null && !PrivateCancelToken.IsCancellationRequested) {
                if(exceptions == null) {
                    PrivateCancelToken.Cancel();
                } else {
                    exceptions.Capture(() => PrivateCancelToken.Cancel());
                }
            }
        }

        protected void TearDownCancellationTokens(List<Exception> exceptions)
        {
            if(exceptions == null) {
                LinkedCancelToken?.Dispose();
                PrivateCancelToken?.Dispose();
                LinkedCancelToken = null;
                PrivateCancelToken = null;
            } else {
                if(LinkedCancelToken != null) {
                    exceptions.Capture(() => LinkedCancelToken.Dispose());
                    LinkedCancelToken = null;
                }
                if(PrivateCancelToken != null) {
                    exceptions.Capture(() => PrivateCancelToken.Dispose());
                    PrivateCancelToken = null;
                }
            }
        }

        /// <summary>
        /// When implemented by a subclass this performs any tear-down required.
        /// </summary>
        protected virtual void TearDownState()
        {
            // never put anything here, we don't want to force derivees to call down to us
        }
    }
}
