// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.QuickInfo {
    public class EditorQuickInfo : IEditorQuickInfo {
        public IEnumerable<string> Content { get; }

        public ITrackingTextRange ApplicableToRange { get; }

        public EditorQuickInfo(IEnumerable<string> content, ITrackingTextRange applicableRange) {
            Content = content;
            ApplicableToRange = applicableRange;
        }
    }
}
