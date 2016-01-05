using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.TestApp {
    public class HeaderProvider : IListProvider<string> {
        private bool _isRow;

        private const string RowFormat = "[{0},]";
        private const string ColumnFormat = "[,{0}]";

        public HeaderProvider(int count, bool isRow) {
            Count = count;
            _isRow = isRow;
        }

        public int Count { get; }

        public async Task<IList<string>> GetRangeAsync(Range range) {
            await Task.Delay(100);

            List<string> list = new List<string>();

            string format = _isRow ? RowFormat : ColumnFormat;
            for (int i = 0; i < range.Count; i++) {
                list.Add(string.Format(format, range.Start + i));
            }

            return list;
        }
    }
}
