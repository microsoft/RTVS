// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static System.FormattableString;

namespace Microsoft.R.Editor.Tree {
    /// <summary>
    /// HTML tree lock. In addition to basic reader/writer lock functionality
    /// it has ability to track lock holders and assert if someone
    /// tries to reacquire lock again and makes it easier to track
    /// who is currently holding the lock.
    /// </summary>
    internal sealed class EditorTreeLock : IDisposable {
        /// <summary>
        /// Locks that controls access to the tree and to its element keys collection.
        /// </summary>
        private ReaderWriterLockSlim _treeLock;
        private int _ownerThreadId;

        /// <summary>
        /// Tracks tree users and helps to debug mismatched lock/unlock bugs.
        /// </summary>
        private HashSet<Guid> _treeUsers;

        public EditorTreeLock() {
            _treeLock = new ReaderWriterLockSlim();
            _ownerThreadId = Thread.CurrentThread.ManagedThreadId;
            _treeUsers = new HashSet<Guid>();
        }

        internal void TakeThreadOwnership() => _ownerThreadId = Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// Acquires read lock on the tree. Must be called before any tree access.
        /// </summary>
        /// <param name="userID">Guid uniquely identifying caller. Used for usage tracking and debugging.</param>
        /// <returns>Tree root if lock was acquired. Null if caller already has the lock.</returns>
        public bool AcquireReadLock(Guid userID) {
            lock (_treeUsers) {
                if (_treeUsers.Contains(userID)) {
                    Debug.Assert(false, String.Empty, "Reentrancy in the EditorTree.AcquireReadLock() is not allowed. User: {0}", userID);
                    return false;
                }
                _treeUsers.Add(userID);
            }

            _treeLock.EnterReadLock();
            return true;
        }

        /// <summary>
        /// Releases read lock previously acquired by <seealso cref="BeginUse"/>
        /// </summary>
        /// <param name="userID">Guid uniquely identifying caller. Used for usage tracking and debugging.</param>
        /// <returns>True if lock was released. False if caller didn't have read lock.</returns>
        public bool ReleaseReadLock(Guid userID) {
            lock (_treeUsers) {
                if (!_treeUsers.Contains(userID)) {
                    Debug.Assert(false, String.Empty, "EditorTree.EndUse() from unknown user: {0}", userID);
                    return false;
                }
                _treeUsers.Remove(userID);
            }

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

        public void Dispose() {
            if (_treeLock != null) {
                Debug.Assert(!_treeLock.IsWriteLockHeld);
                Debug.Assert(!_treeLock.IsReadLockHeld);

                _treeLock.Dispose();
                _treeLock = null;
            }
        }

        public override string ToString()
            => Invariant($"RL:{ _treeLock.CurrentReadCount}, WL:{_treeLock.IsWriteLockHeld}");
    }
}
