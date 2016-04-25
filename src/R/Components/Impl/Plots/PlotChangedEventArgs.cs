// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace Microsoft.R.Components.Plots {
    /// <summary>
    /// provide data ralated to PlotChanged event
    /// </summary>
    public class PlotChangedEventArgs : EventArgs {
        /// <summary>
        /// new plot UIElement
        /// </summary>
        public UIElement NewPlotElement { get; set; }
    }
}