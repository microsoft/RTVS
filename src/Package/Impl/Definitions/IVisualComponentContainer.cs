// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Controls;
using Microsoft.Languages.Editor.Controller;

namespace Microsoft.VisualStudio.R.Package.Definitions {
    /// <summary>
    /// Represents UI element that holds visual component
    /// (typically a tool window)
    /// </summary>
    public interface IVisualComponentContainer<T> where T : IVisualComponent {
        T Component { get; }
    }
}
