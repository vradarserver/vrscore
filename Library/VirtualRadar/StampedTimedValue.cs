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
        public DateTimeOffset LastChanged { get; private set; }

        protected override void SetChangedValue(T newValue, long stamp)
        {
            base.SetChangedValue(newValue, stamp);
            LastChanged = DateTimeOffset.Now;
        }

        public override string ToString() => $"{base.ToString()} @{LastChanged}";

        public override StampedValue<T> ShallowCopy() => CopyTo(new StampedTimedValue<T>());

        public override StampedValue<T> CopyTo(StampedValue<T> obj)
        {
            var other = (StampedTimedValue<T>)obj;
            other.LastChanged = LastChanged;
            return base.CopyTo(obj);
        }
    }
}
