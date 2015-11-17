using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class VariableGrid : ItemsControl {

        static VariableGrid() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VariableGrid), new FrameworkPropertyMetadata(typeof(VariableGrid)));
        }

        public VariableGrid() {
        }

        #region override

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            if (!(newValue is VariableGridDataSource)) {
                throw new NotSupportedException($"JointGrid supports only {typeof(VariableGridDataSource)} for ItemsSource");
            }

            base.OnItemsSourceChanged(oldValue, newValue);

            InitializeDataSource((VariableGridDataSource)newValue);
        }

        #endregion override

        #region PanelInterface

        private VariableGridDataSource _dataSource;
        // TODO: improve to better collection
        private SortedList<int, VariableGridStack> _rows = new SortedList<int, VariableGridStack>();
        private SortedList<int, VariableGridStack> _columns = new SortedList<int, VariableGridStack>();

        private void InitializeDataSource(VariableGridDataSource dataSource) {
            _dataSource = dataSource;
            RowCount = _dataSource.RowCount;
            ColumnCount = _dataSource.ColumnCount;
        }

        public int RowCount { get; private set; }

        public int ColumnCount { get; private set; }

        public VariableGridCell GenerateAt(int rowIndex, int columnIndex, out bool newlyCreated) {
            if (rowIndex < 0 || rowIndex >= RowCount) {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (columnIndex < 0 || columnIndex >= ColumnCount) {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

#if DEBUG && PRINT
            Debug.WriteLine("VariableGridCellGenerator:GenerateAt: {0} {1}", rowIndex, columnIndex);
#endif

            var element = _rows[rowIndex].GetItemAt(columnIndex);
            if (element != null) {
                newlyCreated = false;
                return element;
            }

            element = new VariableGridCell();
            element.Prepare(_dataSource[rowIndex][columnIndex]);
            element.Row = rowIndex;
            element.Column = columnIndex;

            _rows[rowIndex].SetItemAt(columnIndex, element);
            _columns[columnIndex].SetItemAt(rowIndex, element);

            newlyCreated = true;
            return element;
        }

        public bool RemoveAt(int rowIndex, int columnIndex) {
#if DEBUG && PRINT
            Debug.WriteLine("VariableGridCellGenerator:RemoveAt: {0} {1}", rowIndex, columnIndex);
#endif
            var element = _rows[rowIndex].GetItemAt(columnIndex);
            Debug.Assert(object.Equals(element, _columns[columnIndex].GetItemAt(rowIndex)));

            if (element != null) {
                element.CleanUp(_dataSource[rowIndex][columnIndex]);

                _rows[rowIndex].ClearAt(columnIndex);
                _columns[columnIndex].ClearAt(rowIndex);

                return true;
            }

            return false;
        }

        public void RemoveRowsExcept(Range except) {
            RemoveStacksExcept(_rows, except);
        }

        public void RemoveColumnsExcept(Range except) {
            RemoveStacksExcept(_columns, except);
        }

        private void RemoveStacksExcept(SortedList<int, VariableGridStack> stackDictionary, Range except) {
            var toBeDeleted = stackDictionary.Keys.Where(key => !except.Contains(key)).ToList();
            foreach (var row in toBeDeleted) {
                stackDictionary.Remove(row);
            }
        }

        public VariableGridStack GetColumn(int index) {
            return GetStack(_columns, index, Orientation.Vertical);
        }

        public VariableGridStack GetRow(int index) {
            return GetStack(_rows, index, Orientation.Horizontal);
        }

        private VariableGridStack GetStack(SortedList<int, VariableGridStack> stacks, int index, Orientation stackingDirection) {
            VariableGridStack stack;
            if (stacks.TryGetValue(index, out stack)) {
                return stack;
            }

            stack = new VariableGridStack(stackingDirection, index);
            stacks.Add(index, stack);

            return stack;
        }

        public void FreezeLayoutSize() {
            // freeze remaining row and columns
            foreach (var row in _rows.Values) {
                if (row.LayoutSize.Max.HasValue) {
                    row.LayoutSize.Frozen = true;
                }
            }

            foreach (var column in _columns.Values) {
                if (column.LayoutSize.Max.HasValue) {
                    column.LayoutSize.Frozen = true;
                }
            }
        }

        public void ComputeStackPosition(Range viewportRow, Range viewportColumn, out double computedRowOffset, out double computedColumnOffset) {
            if (viewportRow.Count > 0) {
                ComputeStackPosition(_rows, viewportRow.Start, out computedRowOffset);
            } else {
                computedRowOffset = 0;
            }

            if (viewportColumn.Count > 0) {
                ComputeStackPosition(_columns, viewportColumn.Start, out computedColumnOffset);
            } else {
                computedColumnOffset = 0;
            }
        }

        private void ComputeStackPosition(SortedList<int, VariableGridStack> stacks, int startingIndex, out double computedOffset) {
            double offset = 0;
            computedOffset = 0;
            foreach (var key in stacks.Keys) {
                var stack = stacks[key];

                if (key == startingIndex) {
                    computedOffset = offset;
                }

                stack.LayoutPosition = offset;
                offset += stack.LayoutSize.Max.Value;
            }
        }

        #endregion
    }
}
