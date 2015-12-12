using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {

    public class Page2DManager<T> {
        #region synchronized by _syncObj

        private readonly object _syncObj = new object();
        private Dictionary<int, Dictionary<int, Page2D<T>>> _banks = new Dictionary<int, Dictionary<int, Page2D<T>>>();
        private Queue<Page2D<T>> _requests = new Queue<Page2D<T>>();
        private Task _loadTask = null;

        #endregion

        private IGridProvider<T> _itemsProvider;

        public Page2DManager(IGridProvider<T> itemsProvider, int pageSize, TimeSpan timeout, int keepPageCount) {
            if (itemsProvider == null) {
                throw new ArgumentNullException("itemsProvider");
            }
            if (pageSize <= 0) {
                throw new ArgumentOutOfRangeException("pageSize");
            }
            if (keepPageCount <= 0) {
                throw new ArgumentOutOfRangeException("keepPageCount");
            }

            _itemsProvider = itemsProvider;

            PageSize = pageSize;
            Timeout = timeout;
            KeepPageCount = keepPageCount;
        }

        public int RowCount { get { return _itemsProvider.RowCount; } }

        public int ColumnCount { get { return _itemsProvider.ColumnCount; } }

        public int KeepPageCount { get; }

        public int PageSize { get; }

        public TimeSpan Timeout { get; }

        public PageItem<T> GetItem(int row, int column) {
            lock (_syncObj) {
                int rowPageNumber, columnPageNumber;
                ComputePageNumber(row, column, out rowPageNumber, out columnPageNumber);

                Dictionary<int, Page2D<T>> bank;
                Page2D<T> foundPage;
                if (_banks.TryGetValue(rowPageNumber, out bank)) {
                    if (!bank.TryGetValue(columnPageNumber, out foundPage)) {
                        foundPage = CreateEmptyPage(new PageNumber(rowPageNumber, columnPageNumber));
                        bank.Add(columnPageNumber, foundPage);
                    }

                } else {
                    bank = new Dictionary<int, Page2D<T>>();
                    foundPage = CreateEmptyPage(new PageNumber(rowPageNumber, columnPageNumber));
                    bank.Add(columnPageNumber, foundPage);
                    _banks.Add(rowPageNumber, bank);
                }

                return foundPage.GetItem(row, column);
            }
        }

        private Page2D<T> CreateEmptyPage(PageNumber pageNumber) {
            int rowStart, rowCount;
            GetPageInfo(pageNumber.Row, RowCount, out rowStart, out rowCount);

            int columnStart, columnCount;
            GetPageInfo(pageNumber.Column, ColumnCount, out columnStart, out columnCount);

            var range = new GridRange(
                    new Range(rowStart, rowCount),          // row
                    new Range(columnStart, columnCount));   // column

            var page = new Page2D<T>(pageNumber, range);

            lock (_syncObj) {
                _requests.Enqueue(page);
                EnsureLoadTask();
            }

            return page;
        }

        private void ComputePageNumber(int row, int column, out int rowPageNumber, out int columnPageNumber) {
            rowPageNumber = row / PageSize;
            columnPageNumber = column / PageSize;
        }

        private GridRange GetPageRange(PageNumber pageNumber) {
            int start, count;

            GetPageInfo(pageNumber.Row, RowCount, out start, out count);
            Range row = new Range(start, count);

            GetPageInfo(pageNumber.Column, ColumnCount, out start, out count);
            Range column = new Range(start, count);

            return new GridRange(row, column);
        }

        private void GetPageInfo(int pageNumber, int itemCount, out int pageStartIndex, out int pageSize) {
            pageStartIndex = pageNumber * PageSize;

            int end = pageStartIndex + PageSize;
            int count = itemCount;
            if (end > count) {    // last page
                pageSize = count - pageStartIndex;
            } else {
                pageSize = PageSize;
            }
        }

        private void EnsureLoadTask() {
            if (_loadTask == null) {
                lock (_syncObj) {
                    if (_loadTask == null) {
                        _loadTask = Task.Run(async () => await LoadAndCleanPagesAsync());
                    }
                }
            }
        }

        private async Task LoadAndCleanPagesAsync() {
            bool cleanHasRun = false;
            while (true) {
                Page2D<T> page = null;
                lock (_syncObj) {
                    if (_requests.Count == 0) {
                        if (cleanHasRun) {
                            _loadTask = null;
                            return;
                        } else {

                        }
                    } else {
                        page = _requests.Dequeue();
                        Debug.Assert(page != null);
                    }
                }

                if (page != null) {
                    IGrid<T> data = await _itemsProvider.GetRangeAsync(page.Range);

                    page.PopulateData(data);
                } else {
                    CleanOldPages();
                    cleanHasRun = true;
                }
            }
        }

        private void CleanOldPages() {
            foreach (var bank in _banks.Values) {
                while (bank.Count > KeepPageCount) {
                    DateTime lastTime = DateTime.UtcNow - Timeout;
                    IEnumerable<KeyValuePair<int, Page2D<T>>> toRemove;

                    lock (_syncObj) {
                        toRemove = bank.Where(kv => (kv.Value.LastAccessTime < lastTime)).ToList();
                    }

                    foreach (var item in toRemove) {
                        lock (_syncObj) {
                            bank.Remove(item.Key);
                        }
                        // TODO: release hint
                    }
                }
            }
        }
    }
}
