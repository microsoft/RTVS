// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.R.Components.Search;

namespace Microsoft.R.Components.Test.Fakes.Search {
    [ExcludeFromCodeCoverage]
    [Export(typeof(ISearchControlProvider))]
    internal class TestSearchControlProvider : ISearchControlProvider {
        public ISearchControl Create(FrameworkElement host, ISearchHandler handler, SearchControlSettings settings) {
            return new TestSearchControl(handler, settings);
        }
    }
}
