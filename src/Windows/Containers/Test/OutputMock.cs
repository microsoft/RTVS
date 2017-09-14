// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Common.Core.Output;

namespace Microsoft.R.Containers.Windows.Test {
    public class OutputMock : IOutput {
        public void Write(string text) {
            // do nothing
        }
    }
}
