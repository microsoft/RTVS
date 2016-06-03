// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Controller;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotCommands {
        IAsyncCommand CopyAsBitmap { get; }
        IAsyncCommand CopyAsMetafile { get; }
        IAsyncCommand EndLocator { get; }
        IAsyncCommand ExportAsImage { get; }
        IAsyncCommand ExportAsPdf { get; }
        IAsyncCommand Next { get; }
        IAsyncCommand Previous { get; }
        IAsyncCommand RemoveAll { get; }
        IAsyncCommand RemoveCurrent { get; }
    }
}
