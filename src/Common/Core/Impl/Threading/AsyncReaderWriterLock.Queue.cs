// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Common.Core.Threading {
    public partial class AsyncReaderWriterLock {
        internal class Queue<TQueueItem> : IEnumerable<TQueueItem> where TQueueItem: class, IQueueItem {
            private IQueueItem _head;
            private IQueueItem _tail;
            private IQueueItem _wTail;

            /// <summary>
            /// Adds a reader item
            /// </summary>
            /// <remarks>
            /// 1. R−→R−→R  ―►  R−→R−→R−→R
            ///          ↑               ↑
            ///      _tail           _tail
            /// 
            /// 
            /// 2.  _wTail       _wTail
            ///          ↓            ↓
            ///    R−→R−→W  ―►  R−→R−→W−→R
            ///          ↑               ↑
            ///      _tail           _tail
            /// 
            /// 
            /// 3.  _wTail          _wTail
            ///          ↓               ↓
            ///    R−→W−→W−→R  ―►  R−→W−→W−→R−→R
            ///             ↑                  ↑
            ///         _tail              _tail
            /// </remarks>
            /// <param name="item"></param>
            /// <param name="isAddedAfterWriter"></param>
            public void AddReader(TQueueItem item, out bool isAddedAfterWriter) {
                var wTail = _wTail;
                UpdateTail(item);

                var newWTail = SpinWhileWTailIsPendingRemoval();
                isAddedAfterWriter = wTail != newWTail && newWTail != null
                    ? IsFirstEqualOrBeforeSecond(item, newWTail)
                    : newWTail != null;
            }

            private IQueueItem SpinWhileWTailIsPendingRemoval() {
                var wTail = _wTail;
                if (wTail == null || !wTail.IsPendingRemoval) {
                    return wTail;
                }

                var sw = new SpinWait();
                while (wTail != null && wTail.IsPendingRemoval) {
                    sw.SpinOnce();
                    wTail = _wTail;
                }

                return wTail;
            }

            private static bool IsFirstEqualOrBeforeSecond(IQueueItem first, IQueueItem second) {
                while (first != null) {
                    if (first == second) {
                        return true;
                    }
                    first = first.Next;
                }
                return false;
            }

            /// <summary>
            /// Adds a writer item
            /// </summary>
            /// <remarks>
            /// 1.                  _wTail
            ///                          ↓
            ///    R−→R−→R  ―►  R−→R−→R−→W
            ///          ↑               ↑
            ///      _tail           _tail
            /// 
            /// 
            /// 2.  _wTail          _wTail
            ///          ↓               ↓
            ///    R−→R−→W  ―►  R−→R−→W−→W
            ///          ↑               ↑
            ///      _tail           _tail
            /// 
            /// 
            /// 3.  _wTail             _wTail
            ///          ↓                  ↓
            ///    R−→W−→W−→R  ―►  R−→W−→W−→W−→R
            ///          ↑                     ↑
            ///      _tail                 _tail
            /// </remarks>
            /// <param name="item"></param>
            public IQueueItem AddWriter(TQueueItem item) {
                while (true) {
                    var tail = _tail;
                    // Case 1. No writers in the queue. Update _wTail and the _tail
                    var wTail = Interlocked.CompareExchange(ref _wTail, item, null);
                    if (wTail == null) {
                        UpdateTail(item);
                        return item;
                    }

                    // Case 2. No readers in front of a writer, _tail and _wTail point at the same item. Update _wTail.Next, _tail and _wTail
                    if (wTail == tail && wTail.TrySetNext(item, null) == null) {
                        Interlocked.Exchange(ref _tail, item);
                        Interlocked.Exchange(ref _wTail, item);
                        return item;
                    }

                    // Case 3. Readers in front of writers. wTail.Next points to reader. Update item.Next, _wTail.Next and _wTail
                    var wTailNext = wTail.Next;
                    if (wTailNext == null || wTailNext.IsWriter || wTail.IsRemoved) {
                        continue;
                    }

                    item.TrySetNext(wTailNext, item.Next);
                    
                    if (wTail.TrySetNext(item, wTailNext) == wTailNext && !wTail.IsRemoved && Interlocked.CompareExchange(ref _wTail, item, wTail) == wTail) {
                        return item;
                    }
                }
            }
            
            /// <summary>
            /// Updates tail
            /// </summary>
            /// <param name="item">item to set as a tail</param>
            /// <returns>Previous tail</returns>
            private void UpdateTail(TQueueItem item) {
                while (true) {
                    var tail = Interlocked.CompareExchange(ref _tail, item, null);
                    if (tail == null) {
                        Interlocked.Exchange(ref _head, item);
                        return;
                    }
                    
                    if (tail.TrySetNext(item, null) == null && Interlocked.CompareExchange(ref _tail, item, tail) == tail) {
                        return;
                    }
                }
            }

            public void Remove(TQueueItem item) {
                if (item == _head || item == _wTail) {
                    CleanupFromHead();
                }
            }

            private void CleanupFromHead() {
                var head = _head;
                while (head != null && head.IsPendingRemoval) {
                    var next = head.Next;

                    head.MarkRemoved();
                    Interlocked.CompareExchange(ref _tail, null, head);
                    Interlocked.CompareExchange(ref _wTail, null, head);
                    head = Interlocked.CompareExchange(ref _head, next, head) == head ? next : _head;
                }

                if (head == null) {
                    return;
                }

                var item = head;
                var lastWriter = head.IsWriter ? head : null;

                // If new head is pending removal, another thread will handle further cleanup
                while (!head.IsPendingRemoval) {
                    var next = item.Next;
                    if (next == null) {
                       return; 
                    }

                    if (next.IsPendingRemoval) {
                        next.MarkRemoved();
                        Interlocked.CompareExchange(ref _tail, item, next);
                        Interlocked.CompareExchange(ref _wTail, lastWriter, next);
                        item.TrySetNext(next.Next, next);
                        continue;
                    }

                    if (next.IsWriter) {
                        lastWriter = next;
                    }

                    item = next;
                }
            }

            public IEnumerable<TQueueItem> GetFirstReaders() {
                var item = _head;
                while (item != null && (item.IsPendingRemoval || !item.IsWriter)) {
                    if (!item.IsPendingRemoval) {
                        yield return (TQueueItem)item;
                    }

                    item = item.Next;
                }
            }

            public TQueueItem GetFirstAsWriter() {
                var head = _head;
                if (head != null && !head.IsPendingRemoval && head.IsWriter) {
                    return (TQueueItem)head;
                }

                return null;
            }

            public IEnumerator<TQueueItem> GetEnumerator() {
                var item = _head;
                while (item != null) {
                    if (!item.IsPendingRemoval) {
                        yield return (TQueueItem)item;
                    }
                    item = item.Next;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal interface IQueueItem {
            bool IsWriter { get; }
            bool IsPendingRemoval { get; }
            bool IsRemoved { get; }
            IQueueItem Next { get; }

            void MarkRemoved();
            IQueueItem TrySetNext(IQueueItem value, IQueueItem comparand);
        }
    }
}
