using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.TestApp {
    public class ItemsProvider : IGridProvider<GridItem> {
        public ItemsProvider(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Task<IGrid<GridItem>> GetRangeAsync(GridRange gridRange) {
            return Task.Run(async () => {
                await Task.Delay(1000);

                List<GridItem> data = new List<GridItem>(gridRange.Rows.Count * gridRange.Columns.Count);
                for (int c = 0; c < gridRange.Columns.Count; c++) {
                    for (int r = 0; r < gridRange.Rows.Count; r++) {
                        data.Add(
                            new GridItem(
                                r + gridRange.Rows.Start,
                                c + gridRange.Columns.Start));
                    }
                }

                var grid = new Grid<GridItem>(gridRange.Rows.Count, gridRange.Columns.Count, data);
                return (IGrid<GridItem>)grid;
            });
        }
    }
}
