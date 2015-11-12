using System;
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

        private static int RowCount = 100;
        private static int ColumnCount = 100;

        public VariableGridHost() {
            InitializeComponent();

            this.VGrid.ItemsSource = new VariableGridDataSource(RowCount, ColumnCount);
        }
    }
}
