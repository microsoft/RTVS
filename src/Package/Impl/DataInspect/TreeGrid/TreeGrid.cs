using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class TreeGrid : DataGrid {
        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.Key == Key.Right) {
                var node = (ObservableTreeNode) this.SelectedItem;
                node.IsExpanded = true;
            } else if (e.Key == Key.Left) {
                var node = (ObservableTreeNode)this.SelectedItem;
                node.IsExpanded = false;
            }

            base.OnKeyDown(e);
        }
    }
}
