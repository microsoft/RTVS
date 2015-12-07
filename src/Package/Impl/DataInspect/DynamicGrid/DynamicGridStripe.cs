using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class DynamicGridStripe {

        public DynamicGridStripe(Orientation stackingDirection, int index) {
            this.Orientation = stackingDirection;
            this.Index = index;
            this.LayoutSize = new MaxDouble(0.0);
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
        public double LayoutPosition { get; set; }

        /// <summary>
        /// length in layout (perpendicular to stacking direction)
        /// </summary>
        public MaxDouble LayoutSize { get; }

        public double GetSizeConstraint() {
            return double.PositiveInfinity;
            //if (LayoutSize.Frozen) {
            //    return LayoutSize.Max;
            //}

            //return double.PositiveInfinity;
        }


        public object HeaderContent { get; set; }

        public DataTemplate HeaderTemplate { get; set; }
    }
}
