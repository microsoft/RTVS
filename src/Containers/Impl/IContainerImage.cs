// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Containers {
    public interface IContainerImage {
        string Id { get; }
        string Name { get; }
        string Tag { get; }
    }
}
