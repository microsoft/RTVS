// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Windows;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class ScrollCommand {
        public ScrollCommand(GridUpdateType code, double param) {
            Debug.Assert(code != GridUpdateType.SizeChange
                && code != GridUpdateType.SetHorizontalOffset
                && code != GridUpdateType.SetVerticalOffset);

            UpdateType = code;
            Param = param;
        }

        public ScrollCommand(GridUpdateType code, Size size) {
            Debug.Assert(code == GridUpdateType.SizeChange);

            UpdateType = code;
            Param = size;
        }

        public ScrollCommand(GridUpdateType code, double offset, ThumbTrack thumbtrack) {
            Debug.Assert(code == GridUpdateType.SetHorizontalOffset
                || code == GridUpdateType.SetVerticalOffset);

            UpdateType = code;
            Param = new Tuple<double, ThumbTrack>(offset, thumbtrack);
        }

        public GridUpdateType UpdateType { get; set; }

        public object Param { get; set; }
    }
}
