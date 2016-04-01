// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.Search {
    public interface ISearchControlProvider {
        ISearchControl Create(FrameworkElement host, IRPackageManagerViewModel handler, SearchControlSettings settings);
    }
}
