// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace Microsoft.R.Components.View {
    /// <summary>
    /// Represents visual component such a control inside a tool window
    /// </summary>
    public interface IVisualComponent: IDisposable {
        /// <summary>
        /// WPF control to embed in the tool window
        /// </summary>
        FrameworkElement Control { get; }

        /// <summary>
        /// 
        /// </summary>
        IVisualComponentContainer<IVisualComponent> Container { get; }
    }
}
