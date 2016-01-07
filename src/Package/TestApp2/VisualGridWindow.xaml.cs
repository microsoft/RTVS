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
using System.Windows.Shapes;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.TestApp {
    /// <summary>
    /// Interaction logic for VisualGridWindow.xaml
    /// </summary>
    public partial class VisualGridWindow : Window {
        public VisualGridWindow() {
            InitializeComponent();

            RootGrid.Initialize(new DataProvider(1000, 1000));
        }
    }

    class MockRange : IRange<string> {
        private bool _columnMode;
        public MockRange(Range range, bool columnMode) {
            Range = range;
            _columnMode = columnMode;
        }

        public string this[int index] {
            get {
                if (_columnMode) {
                    return string.Format("[{0},]", index);
                }
                return string.Format("[,{0}]", index);
            }

            set {
                throw new NotImplementedException();
            }
        }

        public Range Range { get; }
    }

    class MockGridData : IGridData<string> {
        public MockGridData(GridRange range) {
            ColumnHeader = new MockRange(range.Columns, true);

            RowHeader = new MockRange(range.Rows, false);

            Grid = new Grid<string>(range, (r, c) => string.Format("{0}:{1}", r, c));
        }

        public IRange<string> ColumnHeader { get; private set; }

        public IRange<string> RowHeader { get; private set; }

        public IGrid<string> Grid { get; private set; }

    }

    class DataProvider : IGridProvider<string> {
        public DataProvider(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Task<IGridData<string>> GetAsync(GridRange range) {
            return Task.Run(async () => {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                return (IGridData<string>)new MockGridData(range);
            });
        }

        public Task<IGrid<string>> GetRangeAsync(GridRange gridRange) {
            return Task.Run(async () => {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                return (IGrid<string>)new Grid<string>(gridRange, (r, c) => string.Format("{0}:{1}", r, c));
            });
        }
    }
}
