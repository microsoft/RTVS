// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Forms;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Help {
    public interface IHelpVisualComponent : IVisualComponent {
        /// <summary>
        /// Browser that displays help content
        /// </summary>
        WebBrowser Browser { get; }

        void Navigate(string url);

        string VisualTheme { get; set; }
    }
}
