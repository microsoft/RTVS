using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.TestApp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private static int RowCount = 1000;
        private static int ColumnCount = 1000;

        public MainWindow() {
            InitializeComponent();

            SetDataSources();
        }

        private void SetDataSources() {
            var rowPageManager = new PageManager<string>(
                new HeaderProvider(RowCount, true),
                64,
                TimeSpan.FromMinutes(1.0),
                4);

            var columnPageManager = new PageManager<string>(
                new HeaderProvider(ColumnCount, true),
                64,
                TimeSpan.FromMinutes(1.0),
                4);

            var pageManager = new Page2DManager<GridItem>(
                new ItemsProvider(RowCount, ColumnCount),
                64,
                TimeSpan.FromMinutes(1.0),
                4);


            var gridSource = new DelegateList<DelegateList<PageItem<GridItem>>>(
                0,
                (i) => GetItem(pageManager, i, pageManager.ColumnCount),
                pageManager.RowCount);

            this.GridHost.RowHeaderSource = new DelegateList<PageItem<string>>(0, (i) => rowPageManager.GetItem(i), rowPageManager.Count);
            this.GridHost.ColumnHeaderSource = new DelegateList<PageItem<string>>(0, (i) => columnPageManager.GetItem(i), columnPageManager.Count);
            this.GridHost.ItemsSource = gridSource;
        }

        private static DelegateList<PageItem<GridItem>> GetItem(Page2DManager<GridItem> pm, int rowIndex, int itemCount) {
            return new DelegateList<PageItem<GridItem>>(
                rowIndex,
                (columnIndex) => pm.GetItem(rowIndex, columnIndex),
                itemCount);
        }
    }
}
