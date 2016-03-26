// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class EnumWindowFramesMock : IEnumWindowFrames {
        private List<IVsWindowFrame> _frames;
        private int _index = 0;

        public EnumWindowFramesMock(List<IVsWindowFrame> frames) {
            _frames = frames;
        }

        public int Clone(out IEnumWindowFrames ppenum) {
            throw new NotImplementedException();
        }

        public int Next(uint celt, IVsWindowFrame[] rgelt, out uint pceltFetched) {
            if(_index >= _frames.Count) {
                pceltFetched = 0;
                return VSConstants.S_FALSE;
            }

            rgelt[0] = _frames[_index++];
            pceltFetched = 1;
            return VSConstants.S_OK;
        }

        public int Reset() {
            _index = 0;
            return VSConstants.S_OK;
        }

        public int Skip(uint celt) {
            _index += (int)celt;
            return VSConstants.S_OK;
        }
    }
}
