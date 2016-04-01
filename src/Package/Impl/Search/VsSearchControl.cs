using System;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.R.Components.Search;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Search {
    internal class VsSearchControl : IVsWindowSearch, ISearchControl {
        private readonly IVsWindowSearchHost _vsWindowSearchHost;
        private readonly ISearchHandler _handler;
        private readonly SearchControlSettings _settings;
        private CancellationTokenSource _cts;

        public VsSearchControl(IVsWindowSearchHost vsWindowSearchHost, ISearchHandler handler, SearchControlSettings settings) {
            Category = settings.SearchCategory;
            _settings = settings;
            _handler = handler;
            _vsWindowSearchHost = vsWindowSearchHost;
            _vsWindowSearchHost.SetupSearch(this);
        }

        public IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback) {
            var cts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _cts, cts);
            oldCts?.Cancel();
            return new VsSearchTask(dwCookie, pSearchQuery, pSearchCallback, _handler, cts);
        }

        public void ClearSearch() {
            var cts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _cts, cts);
            oldCts?.Cancel();
            _handler.Search(string.Empty, cts.Token).DoNotWait();
        }

        public void ProvideSearchSettings(IVsUIDataSource pSearchSettings) {
            var settings = (SearchSettingsDataSource)pSearchSettings;
            settings.SearchStartType = VSSEARCHSTARTTYPE.SST_INSTANT;
            SetIfSpecified(settings, SearchSettingsDataSource.ControlMinWidthProperty, nameof(SearchControlSettings.MinWidth));
            SetIfSpecified(settings, SearchSettingsDataSource.ControlMaxWidthProperty, nameof(SearchControlSettings.MaxWidth));
        } 

        private void SetIfSpecified(SearchSettingsDataSource vsSettings, GelProperty vsPropertyName, string propertyName) {
            object value;
            if (_settings.TryGetValue(propertyName, out value)) {
                vsSettings.SetValue(vsPropertyName, value);
            }
        }

        public bool OnNavigationKeyDown(uint dwNavigationKey, uint dwModifiers) => false;
        public bool SearchEnabled => true;
        public Guid Category { get; }
        public IVsEnumWindowSearchFilters SearchFiltersEnum => null;
        public IVsEnumWindowSearchOptions SearchOptionsEnum => null;

        public void Dispose() {
            _vsWindowSearchHost.TerminateSearch();
        }
    }
}