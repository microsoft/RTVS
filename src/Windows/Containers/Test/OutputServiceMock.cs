// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System.Threading;
using Microsoft.R.Common.Core.Output;

namespace Microsoft.R.Containers.Windows.Test {
    public class OutputServiceMock : IOutputService {
        public IOutput Get(string name, CancellationToken cancellationToken) {
            return new OutputMock();
        }
    }
}
