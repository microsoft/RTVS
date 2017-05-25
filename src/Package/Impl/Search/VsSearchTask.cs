using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Components.Search;
using Microsoft.VisualStudio.Shell.Interop;
using static Microsoft.VisualStudio.VSConstants.VsSearchTaskStatus;

namespace Microsoft.VisualStudio.R.Package.Search {
    internal class VsSearchTask : IVsSearchTask {
        private readonly IVsSearchCallback _searchCallback;
        private readonly ISearchHandler _handler;
        private readonly CancellationTokenSource _cts;
        private long _taskStatus;

        public uint Id { get; }
        public int ErrorCode { get; }
        public IVsSearchQuery SearchQuery { get; }
        public uint Status => (uint)_taskStatus;

        public VsSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback, ISearchHandler handler, CancellationTokenSource cts) {
            Check.ArgumentNull(nameof(pSearchQuery), pSearchQuery);
            Check.ArgumentNull(nameof(pSearchCallback), pSearchCallback);
            Check.Argument(nameof(dwCookie), () => dwCookie != VSConstants.VSCOOKIE_NIL);
            Check.ArgumentStringNullOrEmpty(nameof(pSearchQuery), pSearchQuery.SearchString);

            Id = dwCookie;
            SearchQuery = pSearchQuery;
            ErrorCode = 0;

            _searchCallback = pSearchCallback;
            _handler = handler;
            _cts = cts;
            _taskStatus = (long)Created;
        }

        public void Start() {
            if (SetTaskStatus(Started)) {
                _handler.Search(SearchQuery.SearchString, _cts.Token).ContinueWith(SearchCompleted);
            }
        }

        public void Stop() {
            if (SetTaskStatus(Stopped)) {
                _cts.Cancel();
            }
        }

        private void SearchCompleted(Task<int> task) {
            switch (task.Status) {
                case TaskStatus.RanToCompletion:
                    _searchCallback.ReportComplete(this, (uint)task.Result);
                    break;
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    _searchCallback.ReportComplete(this, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool SetTaskStatus(VSConstants.VsSearchTaskStatus status) {
            if (status == Started && !TrySetTaskStatus(status, Created)) {
                return false;
            }

            if (status == Stopped && !TrySetTaskStatus(status, Started) && !TrySetTaskStatus(status, Created)) {
                return false;
            }

            if ((status == Completed || status == Error) && !TrySetTaskStatus(status, Started)) {
                return false;
            }

            return _taskStatus != (long)Stopped;
        }

        private bool TrySetTaskStatus(VSConstants.VsSearchTaskStatus value, VSConstants.VsSearchTaskStatus comparand) {
            var v = (long)value;
            var c = (long)comparand;
            return Interlocked.CompareExchange(ref _taskStatus, v, c) == c;
        }
    }
}