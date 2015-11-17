using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Grid item container that maps to a cell in grid
    /// </summary>
    public class VariableGridCell : ContentControl {
        static VariableGridCell() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VariableGridCell), new FrameworkPropertyMetadata(typeof(VariableGridCell)));
        }

        /// <summary>
        /// Vertical item index
        /// </summary>
        [DefaultValue(-1)]
        public int Row { get; set; }

        /// <summary>
        /// Horizontal item index
        /// </summary>
        [DefaultValue(-1)]
        public int Column { get; set; }

        /// <summary>
        /// Prepare data when realized
        /// </summary>
        /// <param name="item">content data for this cell</param>
        public virtual void Prepare(object item) {
            this.Content = item;
        }

        /// <summary>
        /// Clean up data when virtualized
        /// </summary>
        /// <param name="item">content data for this cell</param>
        public virtual void CleanUp(object item) {
            this.Content = null;
        }
    }
}
