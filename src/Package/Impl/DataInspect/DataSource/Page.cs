using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class Page<T> {
        private List<PageItem<T>> _list;

        public Page(int pageNumber, Range range) {
            PageNumber = pageNumber;
            Range = range;
            Node = new LinkedListNode<Page<T>>(this);
            LastAccessTime = DateTime.MinValue;

            List<PageItem<T>> list = new List<PageItem<T>>(range.Count);
            for (int c = 0; c < range.Count; c++) {
                list.Add(new PageItem<T>(range.Start + c));
            }
            _list = new List<PageItem<T>>(list);
        }

        public int PageNumber { get; }

        public Range Range { get; }

        public LinkedListNode<Page<T>> Node;

        public DateTime LastAccessTime { get; set; }

        public PageItem<T> GetItem(int index) {
            Debug.Assert(Range.Contains(index));

            return _list[index - Range.Start];
        }

        internal void PopulateData(IList<T> data) {
            if (data.Count != Range.Count) {
                throw new ArgumentException("Input data doesn't match with page count");
            }

            for (int r = 0; r < data.Count; r++) {
                _list[r].Data = data[r];
            }
        }
    }
}
