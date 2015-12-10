using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.TestApp {
    public struct GridItem : IIndexedItem {
        public GridItem(int row, int column, bool isDefault = false) {
            Row = row;
            Column = column;
            Default = isDefault;
        }

        public bool Default { get; }

        public int Row { get; }

        public int Column { get; }

        int IIndexedItem.Index
        {
            get
            {
                return Column;
            }
        }

        public override string ToString() {
            if (Default) {
                return "-:-";
            }
            return string.Format("{0}:{1}", Row, Column);
        }
    }
}
