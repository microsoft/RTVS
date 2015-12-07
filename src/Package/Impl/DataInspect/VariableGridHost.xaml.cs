using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Interaction logic for VariableGridHost.xaml
    /// </summary>
    public partial class VariableGridHost : UserControl {
        public VariableGridHost() {
            InitializeComponent();
        }

        private IList _gridItemsSource;
        public IList GridItemsSource
        {
            get { return _gridItemsSource; }
            set
            {
                _gridItemsSource = value;

                // refresh source sources
                // TODO: change more robust to setting order. for now... well fine.
                this.VariableGrid.RowHeaderSource = RowHeaderSource;
                this.VariableGrid.ColumnHeaderSource = ColumnHeaderSource;
                this.VariableGrid.ItemsSource = GridItemsSource;
            }
        }

        public IList RowHeaderSource { get; set; }

        public IList ColumnHeaderSource { get; set; }
    }
}
