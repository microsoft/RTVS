// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace Microsoft.R.Components.Plots.Implementation.View {
    public class PointEventArgs : EventArgs {
        public Point Point { get; }

        public PointEventArgs(Point point) {
            Point = point;
        }
    }
}
