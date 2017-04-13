// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Document;

namespace Microsoft.Markdown.Editor.Document {
    public interface IMdEditorDocument: IEditorDocument {
        IContainedLanguageHandler ContainedLanguageHandler { get; }
    }
}
