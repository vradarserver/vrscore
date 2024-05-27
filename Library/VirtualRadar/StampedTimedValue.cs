namespace VirtualRadar
{
    /// <summary>
    /// As per <see cref="StampedValue{T}"/> but it also keeps track of when it last changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StampedTimedValue<T> : StampedValue<T>
    {
        /// <summary>
        /// Gets the date and time the value was last changed. This might regress
        /// depending on whether the user or the O/S changes the system's real-time
        /// clock.
        /// </summary>
        public DateTime LastChangedUtc { get; private set; }

        protected override void SetChangedValue(T newValue, long stamp)
        {
            base.SetChangedValue(newValue, stamp);
            LastChangedUtc = DateTime.UtcNow;
        }

        public override string ToString() => $"{base.ToString()} @{LastChangedUtc} UTC";

        public override StampedValue<T> ShallowCopy() => CopyTo(new StampedTimedValue<T>());

        public override StampedValue<T> CopyTo(StampedValue<T> obj)
        {
            var other = (StampedTimedValue<T>)obj;
            other.LastChangedUtc = LastChangedUtc;
            return base.CopyTo(obj);
        }
    }
}
