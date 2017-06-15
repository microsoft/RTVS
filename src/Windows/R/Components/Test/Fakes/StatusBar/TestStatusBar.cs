// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Components.StatusBar;

namespace Microsoft.R.Components.Test.Fakes.StatusBar {
    [ExcludeFromCodeCoverage]
    public class TestStatusBar : IStatusBar {
        public IDisposable AddItem(UIElement item) => Disposable.Empty;
        public Task<string> GetTextAsync(CancellationToken ct = new CancellationToken()) => Task.FromResult(string.Empty);
        public Task SetTextAsync(string text, CancellationToken ct = new CancellationToken()) => Task.CompletedTask;

        public Task ReportProgressAsync(string message, uint step, uint totalSteps, bool complete = false, 
            CancellationToken ct = new CancellationToken()) => Task.CompletedTask;
    }
}
