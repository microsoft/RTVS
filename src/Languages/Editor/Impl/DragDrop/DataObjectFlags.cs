// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Editor.DragDrop {
    [Flags]
    public enum DataObjectFlags {
        None = 0,

        /// <summary>
        /// Plain text
        /// </summary>
        Text = 0x0010,

        /// <summary>
        /// Unicode text
        /// </summary>
        Unicode = 0x0020,

        /// <summary>
        /// HTML data object (CF_HTML)
        /// </summary>
        Html = 0x0040,

        /// <summary>
        /// File dropped from desktop
        /// </summary>
        FileDrop = 0x0080,

        MultiUrl = 0x0100,

        /// <summary>
        /// Solution explorer (project) items
        /// </summary>
        ProjectItems = 0x0200
    };
}
