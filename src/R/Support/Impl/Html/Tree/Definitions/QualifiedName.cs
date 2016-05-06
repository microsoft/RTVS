// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Tree {
    /// <summary>
    /// Qualified element or attribute name in a form or prefix:name,
    /// like, for example, asp:button
    /// </summary>
    public class QualifiedName {
        /// <summary>Element prefix</summary>
        public string Prefix { get; set; }

        /// <summary>Element name</summary>
        public string Name { get; set; }

        public QualifiedName(string prefix, string name) {
            Prefix = prefix;
            Name = name;
        }
    }
}
