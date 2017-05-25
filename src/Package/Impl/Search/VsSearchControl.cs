// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.R.Components.Search;
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
            SetDWordBuiltIn(pSearchSettings, "SearchStartType", (uint)VSSEARCHSTARTTYPE.SST_INSTANT);
            SetDWordBuiltInIfSpecified(pSearchSettings, nameof(SearchControlSettings.MinWidth), "ControlMinWidth");
            SetDWordBuiltInIfSpecified(pSearchSettings, nameof(SearchControlSettings.MaxWidth), "ControlMaxWidth");
        }

        private void SetDWordBuiltInIfSpecified(IVsUIDataSource pSearchSettings, string propertyName, string vsPropertyName) {
            uint value;
            if (_settings.TryGetValue(propertyName, out value)) {
                SetDWordBuiltIn(pSearchSettings, vsPropertyName, value);
            }
        }

        private static void SetDWordBuiltIn(IVsUIDataSource pSearchSettings, string vsPropertyName, uint value) {
            pSearchSettings.SetValue(vsPropertyName, new VsUIObject(value, VsUIObject.DWordType, __VSUIDATAFORMAT.VSDF_BUILTIN));
        }

        public bool OnNavigationKeyDown(uint dwNavigationKey, uint dwModifiers) => false;
        public bool SearchEnabled => true;
        public Guid Category { get; }
        public IVsEnumWindowSearchFilters SearchFiltersEnum => null;
        public IVsEnumWindowSearchOptions SearchOptionsEnum => null;

        public void Dispose() {
            _vsWindowSearchHost.TerminateSearch();
        }

        private class VsUIObject : IVsUIObject {
            public static string DWordType = "VsUI.DWord";

            private readonly object _data;
            private readonly string _type;
            private readonly uint _format;
            
            public VsUIObject(object value, string type, __VSUIDATAFORMAT format) {
                _data = value;
                _type = type;
                _format = (uint)format;
            }

            private bool AreEqual(IVsUIObject other) {
                if (ReferenceEquals(this, other)) {
                    return true;
                }

                object otherData;
                Marshal.ThrowExceptionForHR(other.get_Data(out otherData));
                return object.Equals(otherData, _data);
            }

            public int Equals(IVsUIObject pOtherObject, out bool pfAreEqual) {
                pfAreEqual = AreEqual(pOtherObject);
                return VSConstants.S_OK;
            }

            public int get_Data(out object pVar) {
                pVar = _data;
                return VSConstants.S_OK;
            }

            public int get_Format(out uint pdwDataFormat) {
                pdwDataFormat = _format;
                return VSConstants.S_OK;
            }

            public int get_Type(out string pTypeName) {
                pTypeName = _type;
                return VSConstants.S_OK;
            }
        }
    }
}