using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class PageManager<T> {
        #region synchronized by _syncObj

        private object _syncObj = new object();
        private Dictionary<int, Page<T>> _banks = new Dictionary<int, Page<T>>();
        private Queue<Page<T>> _requests = new Queue<Page<T>>();
        private Task _loadTask = null;

        #endregion

        private IListProvider<T> _itemsProvider;

        public PageManager(IListProvider<T> itemsProvider, int pageSize, TimeSpan timeout, int keepPageCount) {
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

        public int Count { get { return _itemsProvider.Count; } }

        public int KeepPageCount { get; }

        public int PageSize { get; }

        public TimeSpan Timeout { get; }

        public PageItem<T> GetItem(int index) {
            lock (_syncObj) {
                int pageNumber = index / PageSize;

                Page<T> foundPage;
                if (!_banks.TryGetValue(pageNumber, out foundPage)) {
                    foundPage = CreateEmptyPage(pageNumber);
                    _banks.Add(pageNumber, foundPage);
                }

                return foundPage.GetItem(index);
            }
        }

        private Page<T> CreateEmptyPage(int pageNumber) {
            int start, count;
            GetPageInfo(pageNumber, Count, out start, out count);

            var range = new Range(start, count);

            var page = new Page<T>(pageNumber, range);

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
                Page<T> page = null;
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
                    await LoadAsync(page);
                } else {
                    CleanOldPages();
                    cleanHasRun = true;
                }
            }
        }

        private void CleanOldPages() {
            while (_banks.Count > KeepPageCount) {
                DateTime lastTime = DateTime.UtcNow - Timeout;
                IEnumerable<KeyValuePair<int, Page<T>>> toRemove;

                lock (_syncObj) {
                    toRemove = _banks.Where(kv => (kv.Value.LastAccessTime < lastTime)).ToList();
                }

                foreach (var item in toRemove) {
                    lock (_syncObj) {
                        _banks.Remove(item.Key);
                    }
                    // TODO: release hint
                }
            }
        }

        private async Task LoadAsync(Page<T> page) {
            var task = Task.Run(() => _itemsProvider.GetRangeAsync(page.Range));

            IList<T> data = await task;

            page.PopulateData(data);
        }
    }
}
