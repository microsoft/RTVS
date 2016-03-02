// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;

namespace Microsoft.Languages.Core.Classification {
    /// <summary>
    /// A token that consists of other tokens. Typically used
    /// in scenarios where one language is embedded into another
    /// but editor projections are not supported yet.
    /// </summary>
    public interface ICompositeToken {
        ReadOnlyCollection<object> TokenList { get; }
        string ContentType { get; }
    }
}
