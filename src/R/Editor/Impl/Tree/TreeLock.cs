// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static System.FormattableString;

namespace Microsoft.R.Editor.Tree {
    /// <summary>
    /// AST lock. In addition to basic reader/writer lock functionality
    /// it has ability to track lock holders and assert if someone
    /// tries to reacquire lock again and makes it easier to track
    /// who is currently holding the lock.
    /// </summary>
    internal sealed class EditorTreeLock : IDisposable {
        private readonly ReaderWriterLockSlim _treeLock = new ReaderWriterLockSlim();
        private int _ownerThreadId = Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// Tracks tree users and helps to debug mismatched lock/unlock bugs.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, int> _treeUsers = new ConcurrentDictionary<Guid, int>();

        internal void TakeThreadOwnership() => _ownerThreadId = Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// Acquires read lock on the tree. Must be called before any tree access.
        /// </summary>
        /// <param name="userID">Guid uniquely identifying caller. Used for usage tracking and debugging.</param>
        /// <returns>Tree root if lock was acquired. Null if caller already has the lock.</returns>
        public bool AcquireReadLock(Guid userID) {
            if (_treeUsers.ContainsKey(userID)) {
                Debug.Assert(false, string.Empty, "Reentrancy in the EditorTree.AcquireReadLock() is not allowed. User: {0}", userID);
                return false;
            }
            _treeUsers[userID] = Thread.CurrentThread.ManagedThreadId;
            _treeLock.EnterReadLock();
            return true;
        }

        /// <summary>
        /// Releases read lock previously acquired by <seealso cref="AcquireReadLock"/>
        /// </summary>
        /// <param name="userID">Guid uniquely identifying caller. Used for usage tracking and debugging.</param>
        /// <returns>True if lock was released. False if caller didn't have read lock.</returns>
        public bool ReleaseReadLock(Guid userID) {
            if (!_treeUsers.ContainsKey(userID)) {
                Debug.Assert(false, string.Empty, "EditorTree.ReleaseReadLock() from unknown user: {0}", userID);
                return false;
            }
            _treeUsers.TryRemove(userID, out var _);
            _treeLock.ExitReadLock();
            return true;
        }

        public bool AcquireWriteLock() {
            if (_ownerThreadId != Thread.CurrentThread.ManagedThreadId || _treeLock.IsWriteLockHeld) {
                Debug.Fail("Wrong thread tries to enter editor tree write lock");
                return false;
            }
            _treeLock.EnterWriteLock();
            return true;
        }

        public bool ReleaseWriteLock() {
            if (_ownerThreadId != Thread.CurrentThread.ManagedThreadId || !_treeLock.IsWriteLockHeld) {
                Debug.Fail("Wrong thread tries to exit editor tree write lock");
                return false;
            }
            _treeLock.ExitWriteLock();
            return true;
        }

        public bool CheckAccess() {
            var caller = Thread.CurrentThread.ManagedThreadId;
            return caller == _ownerThreadId || _treeUsers.Values.ToArray().Contains(caller);
        }

        public void Dispose() {
            if (_treeLock != null) {
                Debug.Assert(!_treeLock.IsWriteLockHeld);
                Debug.Assert(!_treeLock.IsReadLockHeld);

                _treeLock.Dispose();
            }
        }

        public override string ToString()
            => Invariant($"RL:{ _treeLock.CurrentReadCount}, WL:{_treeLock.IsWriteLockHeld}");
    }
}
