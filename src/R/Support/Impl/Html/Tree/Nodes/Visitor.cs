// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Html.Core.Tree.Nodes {
    public interface IHtmlTreeVisitorPattern {
        /// <summary>
        /// Traverses the entire tree invoking provided visitor interface.
        /// Returns true if the whole tree was traversed. Visitor can cancel 
        /// the traversal at any time by returning false from the callback.
        /// </summary>
        bool Accept(IHtmlTreeVisitor visitor, object parameter);

        /// <summary>
        /// Traverses the entire tree invoking provided visitor function.
        /// Returns true if the whole tree was traversed. Visitor can cancel 
        /// the traversal at any time by returning false from the callback.
        /// </summary>
        bool Accept(Func<ElementNode, object, bool> visitor, object parameter);
    }

    public interface IHtmlTreeVisitor {
        bool Visit(ElementNode element, object parameter);
    }
}
