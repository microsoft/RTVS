using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class VariableGridStack {

        public VariableGridStack(Orientation stackingDirection, int index) {
            this.Orientation = stackingDirection;
            this.Index = index;
            this.LayoutSize = new MaxDouble();
        }

        public bool IsColumn {
            get {
                return this.Orientation == Orientation.Vertical;
            }
        }

        public bool IsRow {
            get {
                return this.Orientation == Orientation.Horizontal;
            }
        }

        public Orientation Orientation { get; }

        public int Index { get; }

        /// <summary>
        /// position in layout (perpendicular to stacking direction)
        /// </summary>
        public double? LayoutPosition { get; set; }

        /// <summary>
        /// length in layout (perpendicular to stacking direction)
        /// </summary>
        public MaxDouble LayoutSize { get; }

        public double GetSizeConstraint() {
            if (LayoutSize.Frozen) {
                return LayoutSize.Max.Value;
            }

            return double.PositiveInfinity;
        }


        public object HeaderContent { get; set; }

        public DataTemplate HeaderTemplate { get; set; }

        public void SetItemAt(int index, VariableGridCell item) {
            _cells[index] = item;
        }

        public VariableGridCell GetItemAt(int index) {
            VariableGridCell cell;
            if (_cells.TryGetValue(index, out cell)) {
                return cell;
            }
            return null;
        }

        public void ClearAt(int index) {
            _cells.Remove(index);
        }

        public void ClearItems() {
            _cells.Clear();
        }

        private Dictionary<int, VariableGridCell> _cells = new Dictionary<int, VariableGridCell>();
    }
}
