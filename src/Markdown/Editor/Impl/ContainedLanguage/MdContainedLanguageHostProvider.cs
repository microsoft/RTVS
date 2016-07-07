// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    [Export(typeof(IContainedLanguageHostProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [Name("Visual Studio R Markdown Editor Contained Language Host Provider")]
    [Order(Before = "Default")]
    internal sealed class MdContainedLanguageHostProvider : IContainedLanguageHostProvider {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public MdContainedLanguageHostProvider(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        /// <summary>
        /// Retrieves contained language host for a given text buffer.
        /// </summary>
        /// <param name="textView">Primary text view</param>
        /// <param name="textBuffer">Contained language text buffer</param>
        /// <returns>Contained language host, <seealso cref="IContainedLanguageHost"/></returns>
        public IContainedLanguageHost GetContainedLanguageHost(ITextView textView, ITextBuffer textBuffer) {
            var containedLanguageHost = ServiceManager.GetService<IContainedLanguageHost>(textBuffer);
            if (containedLanguageHost == null) {
                var document = MdEditorDocument.FromTextBuffer(textView.TextDataModel.DocumentBuffer);
                containedLanguageHost = new MdContainedLanguageHost(document, textBuffer, _coreShell);
            }
            return containedLanguageHost;
        }
    }
}
