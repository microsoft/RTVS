// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Common.Core.Diagnostics;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// delegate of getting item and implements IList
    /// </summary>
    /// <typeparam name="T">type of item value</typeparam>
    public class DelegateList<T> : IList<T>, IList, IIndexedItem where T : IIndexedItem {
        #region field and ctor

        private readonly string ReadOnlyExceptionMessage = Invariant($"{typeof(DelegateList<T>)} is read only");
        private readonly Func<int, T> _getItem;

        public DelegateList( int index, Func<int, T> getItem,int count) {
            Check.ArgumentNull(nameof(getItem), getItem);

            Index = index;
            _getItem = getItem;
            Count = count;
        }

        #endregion

        #region public

        public int Index { get; }

        #endregion

        #region Page

        private T GetItem(int index) {
            return _getItem(index);
        }

        #endregion

        #region Collection support (IList<T>, IList)

        public int Count {
            //get {
            //    return _pageManager.ItemsCount;
            //}
            get;
        }

        public T this[int index] {
            get { return GetItem(index); }
            set { throw new NotSupportedException(ReadOnlyExceptionMessage); }
        }


        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        public int IndexOf(T item) {
            return item.Index;
        }

        public bool Contains(T item) {
            return IndexOf(item) != -1;
        }

        public void Add(T item) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public void Clear() {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public void Insert(int index, T item) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public bool Remove(T item) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }
        object IList.this[int index] {
            get { return this[index]; }
            set { throw new NotSupportedException(ReadOnlyExceptionMessage); }
        }

        int IList.Add(object value) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        bool IList.Contains(object value) {
            return Contains((T)value);
        }

        int IList.IndexOf(object value) {
            return IndexOf((T)value);
        }

        void IList.Remove(object value) {
            throw new NotSupportedException(ReadOnlyExceptionMessage);
        }

        void IList.Insert(int index, object value) {
            Insert(index, (T)value);
        }

        public object SyncRoot {
            get { return null; }
        }

        public bool IsSynchronized {
            get { return false; }
        }

        public bool IsReadOnly { get { return true; } }

        public bool IsFixedSize { get { return false; } }

        #endregion
    }
}
