// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Forms;
using Microsoft.VisualStudio.R.Package.Definitions;

namespace Microsoft.VisualStudio.R.Package.Help {
    public interface IHelpWindowVisualComponent : IVisualComponent {
        /// <summary>
        /// Browser that displays help content
        /// </summary>
        WebBrowser Browser { get; }

        void Navigate(string url);

        string VisualTheme { get; set; }
    }
}
