// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.R.Components.InfoBar;

namespace Microsoft.R.Components.Test.Fakes.InfoBar {
    [Export(typeof(IInfoBarProvider))]
    internal class TestInfoBarProvider : IInfoBarProvider {
        public IInfoBar Create(Decorator host) => new TestInfoBar();
    }
}
