// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Windows.Threading;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.R.Packages.Markdown;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Options.R.Editor {
    [Export(typeof(IEditorSettingsStorageProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class RMarkdownEditorSettingsStorageProvider : IEditorSettingsStorageProvider {
        public IEditorSettingsStorage GetSettingsStorage() {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            return MdPackage.Current.LanguageSettingsStorage;
        }
    }
}
