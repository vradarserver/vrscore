// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Tests
{
    /// <summary>
    /// Helps when asserting events in unit tests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventRecorder<T>
        where T: EventArgs
    {
        /// <summary>
        /// Gets the number of times the event has been raised.
        /// </summary>
        public int CallCount => AllSenders.Count;

        private readonly List<object> _AllSenders = [];
        /// <summary>
        /// Gets every sender parameter passed to the event. There will be <see cref="CallCount"/>
        /// entries in the list.
        /// </summary>
        public IReadOnlyList<object> AllSenders => _AllSenders;

        /// <summary>
        /// Gets the latest sender from <see cref="AllSenders"/> or null if <see cref="CallCount"/> is zero.
        /// </summary>
        public object Sender => CallCount == 0 ? null : AllSenders[^1];

        private readonly List<T> _AllArgs = [];
        /// <summary>
        /// Gets every args parameter passed to the event. There will be <see cref="CallCount"/>
        /// entries in the list.
        /// </summary>
        public IReadOnlyList<T> AllArgs => _AllArgs;

        /// <summary>
        /// Gets the latest args from <see cref="AllArgs"/> or null if <see cref="CallCount"/> is zero.
        /// </summary>
        public T Args => CallCount == 0 ? null : AllArgs[^1];

        /// <summary>
        /// Raised by <see cref="Handler"/> whenever the event is raised. Can be used to test the state of
        /// objects when the event was raised.
        /// </summary>
        /// <remarks>
        /// The sender passed to the event is the EventRecorder, <em>not</em> the sender of the original event.
        /// By the time the event is raised the EventRecorder's <see cref="Sender"/> property will be set to the
        /// sender of the original event.
        /// </remarks>
        public event EventHandler<T> EventRaised;

        /// <summary>
        /// Raises <see cref="EventRaised"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnEventRaised(T args)
        {
            EventRaised?.Invoke(this, args);
        }

        /// <summary>
        /// An event handler matching the EventHandler and/or EventHandler&lt;&gt; delegate that can be attached
        /// to an event and record the parameters passed by the code that raises the event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public virtual void Handler(object sender, T args)
        {
            _AllSenders.Add(sender);
            _AllArgs.Add(args);

            OnEventRaised(args);
        }

        /// <summary>
        /// Resets <see cref="CallCount"/>, <see cref="AllSenders"/> and <see cref="AllArgs"/>.
        /// </summary>
        public void Clear()
        {
            _AllSenders.Clear();
            _AllArgs.Clear();
        }
    }
}
