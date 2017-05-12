// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.QuickInfo;

namespace Microsoft.R.Editor.QuickInfo {
    public interface IRFunctionQuickInfo : IEditorQuickInfo {
        string FunctionName { get; }
    }
}
