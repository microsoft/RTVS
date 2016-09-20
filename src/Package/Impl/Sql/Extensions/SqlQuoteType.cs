// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.Sql {
    /// <summary>
    /// Describes selected type of quoting for SQL names with spaces
    /// See https://technet.microsoft.com/en-us/library/ms176027%28v=sql.105%29.aspx
    /// </summary>
    public enum SqlQuoteType {
        None,
        Bracket,
        Quote
    }
}
