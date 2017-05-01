// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IRInteractiveWorkflowOperationsEx : IRInteractiveWorkflowOperations {
        void ExecuteCurrentExpression(ITextView textView, Action<ITextView, ITextBuffer, int> formatDocument);
   }
}