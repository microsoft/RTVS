// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Services {
    /// <summary>
    /// Represents object with properties
    /// </summary>
    public interface IPropertyHolder {
        PropertyDictionary Properties { get; }
    }
}
