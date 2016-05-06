// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Tree {
    /// <summary>
    /// Provides information to HTML tree builder about self-closing and implicitly closing elements
    /// </summary>
    public interface IHtmlClosureProvider {
        /// <summary>
        /// Determines if element is self-closing
        /// </summary>
        /// <param name="name">Element qualified name</param>
        /// <returns>True if element is a self-closing element such as &lt;br>, false otherwise</returns>
        bool IsSelfClosing(string name);

        /// <summary>
        /// Determines of element can be implicitly closed such as &lt;td> or &lt;li> elements
        /// as well as names of container elements that close this element. For example, &lt;tr>
        /// and &lt;table> close &lt;td> element.
        /// </summary>
        /// <param name="name">Element qualified name</param>
        /// <param name="containerElementNames">Collection of qualified closing container element names.</param>
        /// <returns>True if element can be implicitly closed, false otherwise.</returns>
        bool IsImplicitlyClosed(string name, out string[] containerElementNames);
    }
}
