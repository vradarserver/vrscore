namespace VirtualRadar.Collections
{
    public enum QueueFullBehaviour
    {
        /// <summary>
        /// If the queue is full then silently abandon the attempt
        /// to enqueue another item.
        /// </summary>
        DiscardNew,

        /// <summary>
        /// If the queue is full then discard the earliest item on
        /// the queue to make room for the new item.
        /// </summary>
        DiscardEarliest,

        /// <summary>
        /// If the queue is full then replace the latest item with
        /// the new item.
        /// </summary>
        DiscardLatest,

        /// <summary>
        /// If the queue is full then throw a <see cref="QueueFullException"/>.
        /// </summary>
        ThrowException,
    }
}
