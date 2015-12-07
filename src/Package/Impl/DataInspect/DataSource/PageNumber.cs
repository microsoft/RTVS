using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public struct PageNumber {
        public PageNumber(int row, int column) {
            Row = row;
            Column = column;
        }

        public int Row { get; }

        public int Column { get; }
    }
}
