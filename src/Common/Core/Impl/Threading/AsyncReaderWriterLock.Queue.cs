// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;

namespace Microsoft.Common.Core.Threading {
    public partial class AsyncReaderWriterLock {
        private class Queue {
            private LockSource _head;
            private LockSource _tail;
            private LockSource _wTail;

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
            public void AddReader(LockSource item, out bool isAddedAfterWriter) {
                lock (this) {
                    UpdateTail(item);
                    isAddedAfterWriter = _wTail != null;
                }
            }

            /// <summary>
            /// Adds a reader from exclusive reader lock
            /// </summary>
            /// <remarks>
            /// 1.  _wTail         erlTail
            ///          ↓               ↓
            ///    R−→R−→R  ―►  R−→R−→R−→E
            ///          ↑               ↑
            ///      _tail           _tail
            /// 
            /// 
            /// 2.  _wTail       _wTail  erlTail
            ///          ↓            ↓  ↓
            ///    R−→R−→W  ―►  R−→R−→W−→E
            ///          ↑               ↑
            ///      _tail           _tail
            /// 
            /// 
            /// 3.  _wTail          _wTail     erlTail
            ///          ↓               ↓           ↓
            ///    R−→W−→W−→R−→R  ―►  R−→W−→W−→R−→R−→E
            ///                ↑                  ↑
            ///            _tail              _tail
            ///  
            /// 
            /// 4.  _wTail _tail       _wTail    _tail
            ///          ↓     ↓            ↓        ↓
            ///    R−→W−→W−→E−→R  ―►  R−→W−→W−→E−→E−→R
            ///             ↑                     ↑
            ///       erlTail               erlTail
            /// </remarks>
            public void AddExclusiveReader(ExclusiveReaderLockSource item, out bool isAddedAfterWriterOrExclusiveReader) {
                lock (this) {
                    var erlTail = item.ExclusiveReaderLock.Tail;
                    if (erlTail?.Next == null) {
                        UpdateTail(item);
                        isAddedAfterWriterOrExclusiveReader = _wTail != null || erlTail != null;
                    } else {
                        Link(item, erlTail.Next);
                        Link(erlTail, item);
                        isAddedAfterWriterOrExclusiveReader = true;
                    }

                    Interlocked.Exchange(ref item.ExclusiveReaderLock.Tail, item);
                }
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
            /// <param name="isFirstWriter"></param>
            public void AddWriter(LockSource item, out bool isFirstWriter) {
                lock (this) {
                    var wTail = Interlocked.Exchange(ref _wTail, item);

                    // Case 1. No writers in the queue. Update _wTail and the _tail
                    // Case 2. No readers in front of a writer, _tail and _wTail point at the same item. Update _wTail.Next, _tail and _wTail
                    if (wTail == null || wTail == _tail) {
                        UpdateTail(item);
                    } else {
                        Link(item, wTail.Next);
                        Link(wTail, item);
                    }

                    isFirstWriter = _head == item;
                }
            }
            
            public LockSource[] Remove(LockSource item) {
                lock (this) {
                    var next = item.Next;
                    var oldHead = Interlocked.CompareExchange(ref _head, next, item);

                    var previous = item.Previous;
                    Interlocked.CompareExchange(ref _tail, previous, item);
                    Interlocked.CompareExchange(ref _wTail, previous != null && previous.IsWriter ? previous : null, item);

                    var erLock = (item as ExclusiveReaderLockSource)?.ExclusiveReaderLock;
                    if (erLock != null) {
                        var previousErlSource = previous as ExclusiveReaderLockSource;
                        Interlocked.CompareExchange(ref erLock.Tail, previousErlSource?.ExclusiveReaderLock == erLock ? previousErlSource : null, item);
                    }
                    
                    Unlink(item);
                    if (_head == null) {
                        return new LockSource[0];
                    }

                    if (_head.IsWriter && item == oldHead) {
                        return new[] { _head };
                    }

                    if (_wTail == null && next != null) {
                        return FilterAndCopyToArray(next);
                    }

                    if (erLock != null) {
                        return FindFirstExclusiveReader(erLock);
                    }

                    return new LockSource[0];
                }
            }

            private void UpdateTail(LockSource item) {
                var tail = Interlocked.Exchange(ref _tail, item);
                if (tail == null) {
                    Interlocked.Exchange(ref _head, item);
                } else {
                    Link(tail, item);
                }
            }

            private LockSource[] FilterAndCopyToArray(LockSource start) {
                var count = 0;
                var item = start;
                while (item != null) {
                    if (ReaderCanBeReleased(item)) {
                        count++;
                    }
                    item = item.Next;
                }

                var items = new LockSource[count];
                count = 0;
                item = start;
                while (item != null) {
                    if (ReaderCanBeReleased(item)) {
                        items[count] = item;
                        count++;
                    }
                    item = item.Next;
                }

                return items;
            }

            private LockSource[] FindFirstExclusiveReader(ExclusiveReaderLock erLock) {
                if (erLock.Tail == null) {
                    return new LockSource[0];
                }

                var item = _head;
                while (item != erLock.Tail) {
                    if (item.IsWriter) {
                        return new LockSource[0];
                    }

                    if ((item as ExclusiveReaderLockSource)?.ExclusiveReaderLock == erLock) {
                        return new[] { item };
                    }

                    item = item.Next;
                }

                return new[] { item };
            }

            private bool ReaderCanBeReleased(LockSource source) {
                var erlSource = source as ExclusiveReaderLockSource;
                var previousErlSource = source.Previous as ExclusiveReaderLockSource;
                return erlSource == null 
                    || previousErlSource == null 
                    || erlSource.ExclusiveReaderLock != previousErlSource.ExclusiveReaderLock;
            }

            private static void Link(LockSource previous, LockSource next) {
                Interlocked.Exchange(ref previous.Next, next);
                Interlocked.Exchange(ref next.Previous, previous);
            }

            private static void Unlink(LockSource item) {
                if (item.Next != null) {
                    Interlocked.Exchange(ref item.Next.Previous, item.Previous);
                }

                if (item.Previous != null) {
                    Interlocked.Exchange(ref item.Previous.Next, item.Next);
                }

                Interlocked.Exchange(ref item.Next, null);
                Interlocked.Exchange(ref item.Previous, null);
            }
        }
    }
}
