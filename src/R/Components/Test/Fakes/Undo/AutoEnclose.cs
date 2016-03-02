// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.R.Components.Test.Fakes.Undo {
    internal delegate void AutoEncloseDelegate();

    [ExcludeFromCodeCoverage]
    internal class AutoEnclose : IDisposable {
        private AutoEncloseDelegate end;

        public AutoEnclose(AutoEncloseDelegate end) {
            this.end = end;
        }

        public void Dispose() {
            end?.Invoke();

            GC.SuppressFinalize(this);
        }
    }
}
