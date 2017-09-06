// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Common.Core.Output{
    public interface IOutputService {
        Task<IOutput> GetAsync(string name, CancellationToken cancellationToken);
    }
}
