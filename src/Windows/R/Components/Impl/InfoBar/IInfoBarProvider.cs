// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Controls;

namespace Microsoft.R.Components.InfoBar {
    public interface IInfoBarProvider {
        IInfoBar Create(Decorator host);
    }
}