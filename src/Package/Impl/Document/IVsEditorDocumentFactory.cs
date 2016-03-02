// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.EditorFactory;

namespace Microsoft.VisualStudio.R.Package.Document {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Needed for MEF")]
    public interface IVsEditorDocumentFactory : IEditorDocumentFactory { }
}
