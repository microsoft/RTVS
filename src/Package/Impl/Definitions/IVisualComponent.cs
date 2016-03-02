// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Controls;
using Microsoft.Languages.Editor.Controller;

namespace Microsoft.VisualStudio.R.Package.Definitions {
    /// <summary>
    /// Represents visual component such a control inside a tool window
    /// </summary>
    public interface IVisualComponent: IDisposable {
        /// <summary>
        /// Controller to send commands to
        /// </summary>
        ICommandTarget Controller { get; }

        /// <summary>
        /// WPF control to embed in the tool window
        /// </summary>
        Control Control { get; }
    }
}
