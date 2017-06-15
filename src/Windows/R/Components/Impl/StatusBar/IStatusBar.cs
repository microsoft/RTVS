// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.R.Components.StatusBar {
    public interface IStatusBar {
        IDisposable AddItem(UIElement item);
        Task<string> GetTextAsync(CancellationToken ct = default(CancellationToken));
        Task SetTextAsync(string text, CancellationToken ct = default(CancellationToken));
        Task ReportProgressAsync(string message, uint step, uint totalSteps, bool complete = false, CancellationToken ct = default(CancellationToken));
    }
}
