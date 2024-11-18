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
    /// A value that is stamped whenever it is changed.
    /// </summary>
    public class StampedValue<T>
    {
        /// <summary>
        /// The stamp assigned on the last change of value. This can never regress.
        /// </summary>
        public long Stamp { get; private set; }

        /// <summary>
        /// True if the <see cref="Stamp"/> indicates that the value has never been set.
        /// </summary>
        public bool HasNeverBeenSet => Stamp == 0;

        /// <summary>
        /// The value last assigned.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Implicit conversion from a stamped value to the <see cref="Value"/>.
        /// </summary>
        /// <param name="stampedValue"></param>
        public static implicit operator T(StampedValue<T> stampedValue) => stampedValue.Value;

        /// <summary>
        /// Assigns a new value. If the value is unchanged then <see cref="Stamp"/>
        /// is left unchanged. If the value changes then <paramref name="stamp"/>
        /// is assigned to <see cref="Stamp"/>, along with the new value to <see cref="Value"/>.
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="stamp"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public bool Set(T newValue, long stamp)
        {
            var changed = !EqualityComparer<T>.Default.Equals(newValue, Value);
            if(changed) {
                SetChangedValue(newValue, stamp);
            }

            return changed;
        }

        /// <summary>
        /// As per <see cref="Set"/> except it also ignores the new value if it is the
        /// type's default value.
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="stamp"></param>
        /// <returns></returns>
        public bool SetIfNotDefault(T newValue, long stamp)
        {
            return !EqualityComparer<T>.Default.Equals(newValue, default)
                ? Set(newValue, stamp)
                : false;
        }

        /// <summary>
        /// As per <see cref="SetIfNotDefault(T, long)"/> except this takes a value of a different
        /// type and, if that value is not the default for its type, then the conversion value is
        /// called to cast to the type held by the aircraft and then that value is assigned.
        /// </summary>
        /// <typeparam name="TRaw"></typeparam>
        /// <param name="newValue"></param>
        /// <param name="stamp"></param>
        /// <param name="toValue"></param>
        /// <returns></returns>
        public bool SetIfNotDefault<TRaw>(TRaw newValue, long stamp, Func<TRaw, T> toValue)
        {
            return !EqualityComparer<TRaw>.Default.Equals(newValue, default)
                ? Set(toValue(newValue), stamp)
                : false;
        }

        protected virtual void SetChangedValue(T newValue, long stamp)
        {
            if(Stamp > stamp) {
                throw new InvalidOperationException($"An attempt was made to regress a stamp on a {nameof(StampedValue<T>)}");
            }
            if(Stamp == stamp) {
                throw new InvalidOperationException($"An attempt was made to change a value without assigning a new stamp on a {nameof(StampedValue<T>)}");
            }
            Value = newValue;
            Stamp = stamp;
        }

        /// <summary>
        /// If the stamp passed across is lower than <see cref="Stamp"/> then the <see cref="unchangedValue"/>
        /// is returned, otherwise <see cref="Value"/> is returned.
        /// </summary>
        /// <param name="previousValueStamp"></param>
        /// <param name="unchangedValue"></param>
        /// <returns></returns>
        public T ValueIfChanged(long previousValueStamp, T unchangedValue = default)
        {
            return Stamp > previousValueStamp
                ? Value
                : unchangedValue;
        }

        /// <summary>
        /// As per <see cref="ValueIfChanged(long, T)"/> except this takes a callback to convert the aircraft
        /// value to another type. The callback is only called when the stamp indicates that the value has
        /// changed.
        /// </summary>
        /// <typeparam name="TCon"></typeparam>
        /// <param name="previousValueStamp"></param>
        /// <param name="toValue"></param>
        /// <returns></returns>
        public TCon ValueIfChanged<TCon>(long previousValueStamp, Func<T, TCon> toValue, TCon unchangedValue = default)
        {
            return Stamp > previousValueStamp
                ? toValue(Value)
                : unchangedValue;
        }

        /// <inheritdoc/>
        public override string ToString() => $"[{Stamp}]:{Value}";

        /// <summary>
        /// Returns a shallow copy of the object.
        /// </summary>
        /// <returns></returns>
        public virtual StampedValue<T> ShallowCopy() => CopyTo(new());

        /// <summary>
        /// Copies the content to another stamped value.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual StampedValue<T> CopyTo(StampedValue<T> obj)
        {
            obj.Stamp = Stamp;
            obj.Value = Value;

            return obj;
        }
    }
}
