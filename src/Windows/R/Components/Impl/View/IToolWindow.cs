// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.View {
    public interface IToolWindow : IDisposable {
        void Show(bool focus, bool immediate);
    }
}