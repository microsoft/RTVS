// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.R.Components.Search;

namespace Microsoft.R.Components.Test.Fakes.Search {
    [ExcludeFromCodeCoverage]
    internal class TestSearchControl : ISearchControl {
        private readonly ISearchHandler _handler;
        private readonly SearchControlSettings _settings;
        private CancellationTokenSource _cts;

        public TestSearchControl(ISearchHandler handler, SearchControlSettings settings) {
            Category = settings.SearchCategory;
            _settings = settings;
            _handler = handler;
        }

        public void ClearSearch() {
            var cts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _cts, cts);
            oldCts?.Cancel();
            _handler.Search(string.Empty, cts.Token).DoNotWait();
        }

        public bool OnNavigationKeyDown(uint dwNavigationKey, uint dwModifiers) => false;
        public bool SearchEnabled => true;
        public Guid Category { get; }

        public void Dispose() {
        }
    }
}