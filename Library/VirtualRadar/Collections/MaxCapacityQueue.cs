// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Collections
{
    /// <summary>
    /// A thread-safe queue-like collection that has a maximum number of entries. Does not support
    /// enumeration. Does not implement any collection or enumerable interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MaxCapacityQueue<T>
    {
        private object _SyncLock = new();
        private readonly LinkedList<T> _LinkedList = new();

        private int _MaximumCapacity = 1000;
        /// <summary>
        /// The maximum number of entries that the queue can accept.
        /// </summary>
        public int MaximumCapacity => _MaximumCapacity;

        public int Count
        {
            get {
                lock(_SyncLock) return _LinkedList.Count;
            }
        }

        private long _CountDiscards;
        /// <summary>
        /// The number of items that have been discarded because the queue was full.
        /// </summary>
        public long CountDiscards => _CountDiscards;

        private DateTimeOffset _LastDicardTime;
        /// <summary>
        /// The time of the last discard.
        /// </summary>
        public DateTimeOffset LastDiscardTime => _LastDicardTime;

        /// <summary>
        /// Adds an item to the queue.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="fullBehaviour">What to do if the queue is already full.</param>
        /// <returns>False if the queue was full and the item was rejected.</returns>
        /// <exception cref="QueueFullException">
        /// Thrown if the queue is full and <paramref name="fullBehaviour"/> is <see
        /// cref="QueueFullBehaviour.ThrowException"/>.
        /// </exception>
        public bool Enqueue(T item, QueueFullBehaviour fullBehaviour = QueueFullBehaviour.DiscardNew)
        {
            var result = false;

            lock(_SyncLock) {
                if(Count >= MaximumCapacity) {
                    switch(fullBehaviour) {
                        case QueueFullBehaviour.DiscardEarliest:
                            TrimQueue(removeEarliest: true, makeRoomForNewEntry: true);
                            break;
                        case QueueFullBehaviour.DiscardLatest:
                            TrimQueue(removeEarliest: false, makeRoomForNewEntry: true);
                            break;
                        case QueueFullBehaviour.ThrowException:
                            throw new QueueFullException($"Attempt made to add entry {_LinkedList.Count + 1} to a queue that is limited to {MaximumCapacity}");
                        case QueueFullBehaviour.DiscardNew:
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                if(Count >= MaximumCapacity) {
                    RecordDiscards(1);
                } else {
                    _LinkedList.AddLast(item);
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Pops the earliest item on the queue.
        /// </summary>
        /// <returns>The earliest item on the queue.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the queue is empty.</exception>
        public T Dequeue()
        {
            lock(_SyncLock) {
                if(_LinkedList.Count == 0) {
                    throw new InvalidOperationException("Attempt made to dequeue from an empty queue");
                }
                var result = _LinkedList.First();
                _LinkedList.RemoveFirst();

                return result;
            }
        }

        /// <summary>
        /// Pops the earliest item off the queue.
        /// </summary>
        /// <param name="result">Set to the earliest item on the queue or default if the queue is empty.</param>
        /// <returns>False if the queue is empty.</returns>
        public bool TryDequeue(out T result)
        {
            var outcome = false;

            lock(_SyncLock) {
                if(_LinkedList.Count == 0) {
                    result = default;
                } else {
                    result = _LinkedList.First();
                    _LinkedList.RemoveFirst();
                    outcome = true;
                }
            }

            return outcome;
        }

        /// <summary>
        /// Returns the earliest entry in the queue without removing it.
        /// </summary>
        /// <returns>The earliest entry in the queue.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the queue is empty.</exception>
        public T Peek()
        {
            lock(_SyncLock) {
                if(_LinkedList.Count == 0) {
                    throw new InvalidOperationException("Attempt made to peek an empty queue");
                }
                return _LinkedList.First();
            }
        }

        /// <summary>
        /// Fetches the earliest entry in the queue without removing it.
        /// </summary>
        /// <param name="result">Set to the earliest entry in the queue or default if the queue is empty.</param>
        /// <returns>False if the queue was empty.</returns>
        public bool TryPeek(out T result)
        {
            var outcome = false;

            lock(_SyncLock) {
                if(_LinkedList.Count == 0) {
                    result = default;
                } else {
                    result = _LinkedList.First();
                    outcome = true;
                }
            }

            return outcome;
        }

        /// <summary>
        /// Sets the maximum capacity of the queue, optionally trimming excess if the
        /// queue is already over the new capacity.
        /// </summary>
        /// <param name="maximumCapacity"></param>
        /// <param name="trimBehaviour"></param>
        public void SetMaximumCapacity(int maximumCapacity, QueueTrimBehaviour trimBehaviour = QueueTrimBehaviour.DoNothing)
        {
            lock(_SyncLock) {
                _MaximumCapacity = maximumCapacity;
                if(_LinkedList.Count > MaximumCapacity) {
                    switch(trimBehaviour) {
                        case QueueTrimBehaviour.DoNothing:
                            break;
                        case QueueTrimBehaviour.TrimFromStart:
                            TrimQueue(removeEarliest: true, makeRoomForNewEntry: false);
                            break;
                        case QueueTrimBehaviour.TrimFromEnd:
                            TrimQueue(removeEarliest: false, makeRoomForNewEntry: false);
                            break;
                        case QueueTrimBehaviour.ThrowException:
                            throw new QueueFullException($"A queue that contained {_LinkedList.Count} item(s) has had its maximum capacity set to {MaximumCapacity}");
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        /// <summary>
        /// Empties the queue.
        /// </summary>
        public void Clear()
        {
            lock(_SyncLock) {
                _LinkedList.Clear();
            }
        }

        /// <summary>
        /// Returns true if the item exists within the queue.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            lock(_SyncLock) {
                return _LinkedList.Contains(item);
            }
        }

        /// <summary>
        /// Removes the item from the queue.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was removed from the queue.</returns>
        public bool Remove(T item)
        {
            lock(_SyncLock) {
                return _LinkedList.Remove(item);
            }
        }

        private void TrimQueue(bool removeEarliest, bool makeRoomForNewEntry)
        {
            var trimCount = Math.Min(Count, makeRoomForNewEntry
                ? Count - (MaximumCapacity - 1)
                : Count - MaximumCapacity
            );

            if(trimCount > 0) {
                for(var idx = 0;idx < trimCount;++idx) {
                    if(removeEarliest) {
                        _LinkedList.RemoveFirst();
                    } else {
                        _LinkedList.RemoveLast();
                    }
                }
                RecordDiscards(trimCount);
            }
        }

        private void RecordDiscards(long discards)
        {
            if(discards > 0) {
                _CountDiscards += discards;
                _LastDicardTime = DateTimeOffset.Now;
            }
        }
    }
}
