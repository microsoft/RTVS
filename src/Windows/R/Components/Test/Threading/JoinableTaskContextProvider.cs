// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.R.Components.Test.Threading {
    [Export]
    [ExcludeFromCodeCoverage]
    public sealed class JoinableTaskContextProvider {
        [Export]
        private JoinableTaskContext UIThreadContext { get; set; }

        public JoinableTaskContextProvider() {
            UIThreadContext = new JoinableTaskContext(UIThreadHelper.Instance.Thread);
        }
    }
}
