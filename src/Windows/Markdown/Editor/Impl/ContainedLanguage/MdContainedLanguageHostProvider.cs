// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Document;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    [Export(typeof(IContainedLanguageHostProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [Name("Visual Studio R Markdown Editor Contained Language Host Provider")]
    [Order(Before = "Default")]
    internal sealed class MdContainedLanguageHostProvider : IContainedLanguageHostProvider {
        /// <summary>
        /// Retrieves contained language host for a given text buffer.
        /// </summary>
        /// <param name="editorView">Primary text view</param>
        /// <param name="editorBuffer">Contained language text buffer</param>
        /// <returns>Contained language host, <seealso cref="IContainedLanguageHost"/></returns>
        public IContainedLanguageHost GetContainedLanguageHost(IEditorView editorView, IEditorBuffer editorBuffer) {
            var containedLanguageHost = editorBuffer.GetService<IContainedLanguageHost>();
            if (containedLanguageHost == null) {
                var document = editorView.EditorBuffer.GetEditorDocument<IMdEditorDocument>();
                Debug.Assert(document != null);
                containedLanguageHost = new MdContainedLanguageHost(document, editorBuffer);
            }
            return containedLanguageHost;
        }
    }
}
