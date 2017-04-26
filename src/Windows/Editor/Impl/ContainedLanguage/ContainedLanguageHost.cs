// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Helper class that simplifies location of contained language host
    /// for contained (secondary) languages. 
    /// </summary>
    public static class ContainedLanguageHost {
        /// <summary>
        /// Retrives contained language host. Typically called from text view connection listener.
        /// </summary>
        /// <param name="textView">Primary text view</param>
        /// <param name="textBuffer">Contained language buffer</param>
        /// <param name="services"></param>
        /// <returns>Contained language host for this buffer and language, <seealso cref="IContainedLanguageHost"/></returns>
        public static IContainedLanguageHost GetHost(ITextView textView, ITextBuffer textBuffer, IServiceContainer services) {
            var containedLanguageHost = TryGetHost(textBuffer);
            if (containedLanguageHost == null) {
                var containedLanguageHostProvider =
                    ComponentLocatorForOrderedContentType<IContainedLanguageHostProvider>.
                            FindFirstOrderedComponent(services.GetService<ICompositionCatalog>(), textView.TextDataModel.DocumentBuffer.ContentType.TypeName);
                containedLanguageHost = containedLanguageHostProvider?.GetContainedLanguageHost(textView.ToEditorView(), textBuffer.ToEditorBuffer());
            }
            return containedLanguageHost;
        }

        /// <summary>
        /// Retrives contained language host if already created.
        /// </summary>
        /// <param name="textBuffer">Contained language buffer</param>
        /// <returns>Contained language host for this buffer and language, <seealso cref="IContainedLanguageHost"/> if it exists</returns>
        public static IContainedLanguageHost TryGetHost(ITextBuffer textBuffer) => textBuffer.GetService<IContainedLanguageHost>();
    }
}
