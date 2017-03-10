// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Services {
    public interface IServiceFactory {
        object CreateService(params object[] arguments);
    }
}
