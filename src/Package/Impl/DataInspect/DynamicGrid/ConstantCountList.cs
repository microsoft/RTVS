using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// A utility IList collection that returns constant number of items
    /// </summary>
    /// <typeparam name="T">item type</typeparam>
    public abstract class ConstantCountList<T> : IList<T>, IList {
        public ConstantCountList(int count) : this(0, count) { }

        public ConstantCountList(int startIndex, int count) {
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count");
            }
            this.StartIndex = startIndex;
            this.Count = count;
        }

        public int StartIndex { get; private set; }

        public abstract T this[int index] { get; set; }

        object IList.this[int index] {
            get {
                return this[index];
            }

            set {
                this[index] = (T)value;
            }
        }

        public int Count { get; private set; }

        public bool IsFixedSize { get { return true; } }

        public bool IsReadOnly { get { return true; } }

        public bool IsSynchronized { get { return false; } }

        public object SyncRoot { get { return null; } }

        public int Add(object value) {
            throw new NotSupportedException($"{typeof(ConstantCountList<T>)} doesn't support Add");
        }

        public void Add(T item) {
            throw new NotSupportedException($"{typeof(ConstantCountList<T>)} doesn't support Add");
        }

        public void Clear() {
            throw new NotSupportedException($"{typeof(ConstantCountList<T>)} doesn't support Clear");
        }

        public bool Contains(object value) {
            return Contains((T)value);
        }

        public bool Contains(T item) {
            return IndexOf(item) != -1;
        }

        public void CopyTo(Array array, int index) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            if (index < 0) {
                throw new ArgumentOutOfRangeException("index", "Array index must be great than or equal to zero");
            }
            if (index + Count > array.Length) {
                throw new ArgumentException("Copy destination array is shorter than the source");
            }

            for (int i = 0; i < Count; i++) {
                array.SetValue(this[i], index + i);
            }
        }

        public void CopyTo(T[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex < 0) {
                throw new ArgumentOutOfRangeException("arrayIndex", "Array index must be great than or equal to zero");
            }
            if (arrayIndex + Count > array.Length) {
                throw new ArgumentException("Copy destination array is shorter than the source");
            }

            for (int i = 0; i < Count; i++) {
                array[arrayIndex + i] = this[i];
            }
        }

        public virtual IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        public int IndexOf(object value) {
            return IndexOf((T)value);
        }

        public abstract int IndexOf(T item);

        public void Insert(int index, object value) {
            throw new NotSupportedException($"{typeof(ConstantCountList<T>)} doesn't support Insert");
        }

        public void Insert(int index, T item) {
            throw new NotSupportedException($"{typeof(ConstantCountList<T>)} doesn't support Insert");
        }

        public void Remove(object value) {
            throw new NotSupportedException($"{typeof(ConstantCountList<T>)} doesn't support Remove");
        }

        public bool Remove(T item) {
            throw new NotSupportedException($"{typeof(ConstantCountList<T>)} doesn't support Remove");
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException($"{typeof(ConstantCountList<T>)} doesn't support RemoveAt");
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
