﻿// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Filtering
{
    /// <summary>
    /// An object that carries a bool filter.
    /// </summary>
    public class FilterBool : Filter
    {
        /// <summary>
        /// Gets or sets the value to compare against.
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FilterBool() : base()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="value"></param>
        public FilterBool(bool value) : this()
        {
            Value = value;
            Condition = FilterCondition.Equals;
        }

        /// <summary>
        /// Returns true if the value passes the filter.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Passes(bool? value)
        {
            var result = true;

            if(Condition == FilterCondition.Equals) {
                result = value != null;
                if(result) {
                    result = Value == value.Value;
                }
                if(ReverseCondition) {
                    result = !result;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var result = base.Equals(obj);
            if(result) {
                result = false;
                if(obj is FilterBool other) {
                    result = Value == other.Value;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        // Do not use these objects as keys!
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Value);

        /// <inheritdoc/>
        public override string ToString() => $"{base.ToString()} {Value}";
    }
}
