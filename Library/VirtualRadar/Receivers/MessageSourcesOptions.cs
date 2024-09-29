// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Configuration;

namespace VirtualRadar.Receivers
{
    /// <summary>
    /// Describes the options for all sources of aircraft messages.
    /// </summary>
    /// <remarks>
    /// This is a class because .NET 8 records equality comparisons don't work with arrays
    /// and objects. If the situation changes in later versions of .NET then we can cut a
    /// lot of code out of this.
    /// </remarks>
    [Settings("MessageSources")]
    public class MessageSourcesOptions
    {
        private ReceiverOptions[] _Receivers = [];
        public ReceiverOptions[] Receivers { get; set; }

        public MessageSourcesOptions()
        {
        }

        public MessageSourcesOptions(ReceiverOptions[] receivers)
        {
            _Receivers = receivers;
        }

        public MessageSourcesOptions(MessageSourcesOptions source) : this(
            receivers: source.Receivers
        )
        {
        }

        public static bool operator==(MessageSourcesOptions lhs, MessageSourcesOptions rhs) => Object.Equals(lhs, rhs);

        public static bool operator!=(MessageSourcesOptions lhs, MessageSourcesOptions rhs) => !Object.Equals(lhs, rhs);

        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is MessageSourcesOptions other) {
                result = Receivers.SequenceEqual(other.Receivers);
            }

            return result;
        }

        public override int GetHashCode() => Receivers.Length.GetHashCode();

        public override string ToString()
        {
            var result = new StringBuilder();

            result.AppendLine($"{nameof(MessageSourcesOptions)} {{ {nameof(Receivers)} = [");
            foreach(var receiver in Receivers) {
                result.AppendLine($"  {receiver}");
            }
            result.AppendLine($"] }}");

            return result.ToString();
        }
    }
}
