// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.QuickInfo {
    public interface IEditorQuickInfo {
        /// <summary>
        /// Quick info content
        /// </summary>
        IEnumerable<string> Content { get; }

        /// <summary>
        /// Span of text in the buffer to which this quick info is applicable.
        /// </summary>
        ITrackingTextRange ApplicableToRange { get; }
    }
}
