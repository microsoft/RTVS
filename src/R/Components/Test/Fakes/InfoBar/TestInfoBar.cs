// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Components.InfoBar;

namespace Microsoft.R.Components.Test.Fakes.InfoBar {
    internal class TestInfoBar : IInfoBar {
        public IDisposable Add(InfoBarItem item) => Disposable.Empty;
    }
}