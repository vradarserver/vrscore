namespace VirtualRadar.Collections
{
    /// <summary>
    /// Can be thrown by <see cref="MaxCapacityQueue{T}"/> when an attempt is
    /// made to add another entry to a full queue.
    /// </summary>
    [Serializable]
    public class QueueFullException : Exception
    {
        public QueueFullException() { }

        public QueueFullException(string message) : base(message) { }

        public QueueFullException(string message, Exception inner) : base(message, inner) { }
    }
}
