using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class VariableGridDataSource : ConstantCountList<IntegerList> {  // TODO: change from IntegerList to generic
        public VariableGridDataSource(int rowCount, int columnCount) : base(rowCount) {
            if (rowCount < 0) {
                throw new ArgumentOutOfRangeException("rowCount");
            }
            if (columnCount < 0) {
                throw new ArgumentOutOfRangeException("columnCount");
            }

            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int RowCount { get; }
        public int ColumnCount { get; }

        #region ConstantCountList abstract implementation

        public override IntegerList this[int index] {
            get {
                return new IntegerList(ColumnCount * index, ColumnCount);
            }

            set {
                throw new NotSupportedException($"{typeof(VariableGridDataSource)} is read only");
            }
        }

        public override int IndexOf(IntegerList item) {
            if (item.Start >= 0 && item.Count == ColumnCount) {
                int remainder;
                int rowIndex = Math.DivRem(item.Start, ColumnCount, out remainder);
                if (remainder == 0 && rowIndex < RowCount) {
                    return rowIndex;
                }
            }
            return -1;
        }

        #endregion
    }

    /// <summary>
    /// A simulated collection that provides integer range, TEMPORARY. WILL BE DELETED
    /// </summary>
    public class IntegerList : IList<int>, IList {
        public IntegerList(int start, int count) {
            Start = start;
            Count = count;
        }

        public int Start { get; }

        #region IList support

        public int this[int index] {
            get {
                //Debug.WriteLine($"IntegerList[{Start}][{index}] getter");
                return index + Start;
            }

            set {
                throw new NotSupportedException($"{typeof(IntegerList)} doesn't support assigning item's value");
            }
        }

        public int Count { get; }

        public bool IsReadOnly { get { return true; } }

        public bool IsFixedSize { get { return true; } }

        public object SyncRoot { get { return null; } }

        public bool IsSynchronized { get { return false; } }

        object IList.this[int index] {
            get {
                return this[index];
            }

            set {
                this[index] = (int)value;
            }
        }

        public void Add(int item) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support adding new item");
        }

        public void Clear() {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support clearing items");
        }

        public bool Contains(int item) {
            return (item >= Start && item < (Start + Count));
        }

        public void CopyTo(int[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public IEnumerator<int> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        public int IndexOf(int item) {
            if (!Contains(item)) return -1;

            return item - Start;
        }

        public void Insert(int index, int item) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support inserting item");
        }

        public bool Remove(int item) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support removing item");
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support removing item");
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Add(object value) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support adding item");
        }

        public bool Contains(object value) {
            if (value is int) {
                return Contains((int)value);
            }
            return false;
        }

        public int IndexOf(object value) {
            if (value is int) {
                return IndexOf((int)value);
            }
            return -1;
        }

        public void Insert(int index, object value) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support inserting item");
        }

        public void Remove(object value) {
            throw new NotSupportedException($"{typeof(IntegerList)} doesn't support removing item");
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        #endregion IList support
    }
}
