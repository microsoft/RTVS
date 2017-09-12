// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;

namespace Microsoft.R.Common.Core.Output{
    public interface IOutputService {
        IOutput Get(string name, CancellationToken cancellationToken);
    }
}
