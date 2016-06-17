// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    public sealed class VsEnumStreamMarkersMock : IVsEnumStreamMarkers {
        private readonly IVsTextStreamMarker[] _markers;
        private int _index;

        public VsEnumStreamMarkersMock(IVsTextStreamMarker[] markers) {
            _markers = markers;
        }

        public int GetCount(out int pcMarkers) {
            pcMarkers = _markers.Length;
            return VSConstants.S_OK;
        }

        public int Next(out IVsTextStreamMarker ppRetval) {
            ppRetval = null;
            if (_index >= 0 && _index < _markers.Length) {
                ppRetval = _markers[_index];
                _index++;
                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }

        public int Reset() {
            _index = 0;
            return VSConstants.S_OK;
        }
    }
}
