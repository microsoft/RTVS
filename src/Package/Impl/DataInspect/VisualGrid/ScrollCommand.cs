// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Windows;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class ScrollCommand {
        public ScrollCommand(GridUpdateType code, object param) {
            UpdateType = code;
            Param = param;
        }

        public GridUpdateType UpdateType { get; set; }
        public object Param { get; set; }
    }
}
