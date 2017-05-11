// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Editor.Services;

namespace Microsoft.Languages.Editor.Settings {
    public abstract class EditorSettings : IWritableEditorSettings, IEditorSettings {
        public const string AutoFormatKey = "AutoFormat";
        public const string CompletionEnabledKey = "CompletionEnabled";
        public const string FormatterIndentSizeKey = "FormatterIndentSize";
        public const string FormatterTabSizeKey = "FormatterTabSize";
        public const string FormatterIndentTypeKey = "FormatterIndentType";
        public const string IndentStyleKey = "IndentStyle";
        public const string SyntaxCheckKey = "SyntaxCheckEnabled";
        public const string InsertMatchingBracesKey = "InsertMatchingBraces";
        public const string SignatureHelpEnabledKey = "SignatureHelpEnabled";

        private readonly ICoreShell _coreShell;
        private readonly string _language;
        private IEditorSettingsStorage _storage;

        protected IEditorSettingsStorage Storage => GetStorage();
        protected IWritableEditorSettingsStorage WritableStorage => Storage as IWritableEditorSettingsStorage;

        protected EditorSettings(ICoreShell coreShell, string language) {
            _coreShell = coreShell;
            _language = language;
        }

        protected EditorSettings(IEditorSettingsStorage storage) {
            _storage = storage;
        }

        private IEditorSettingsStorage GetStorage() {
            if (_storage == null) {
                var locator = _coreShell.GetService<IContentTypeServiceLocator>();
                var provider = locator.GetService<IEditorSettingsStorageProvider>(_language);
                Debug.Assert(provider != null, "No editor storage found for language " + _language);
                _storage = provider.GetSettingsStorage();
            }
            return _storage;
        }

        public void Dispose() => _storage?.Dispose();

        public event EventHandler<EventArgs> SettingsChanged {
            add => Storage.SettingsChanged += value;
            remove => Storage.SettingsChanged -= value;
        }

        public virtual void ResetSettings() => WritableStorage?.ResetSettings();

        public bool AutoFormat {
            get => Storage.Get(AutoFormatKey, true);
            set => WritableStorage?.Set(AutoFormatKey, value);
        }

        public bool CompletionEnabled {
            get => Storage.Get(CompletionEnabledKey, true);
            set => WritableStorage?.Set(CompletionEnabledKey, value);
        }

        public int IndentSize {
            get => Storage.Get(FormatterIndentSizeKey, 4);
            set => WritableStorage?.Set(FormatterIndentSizeKey, value);
        }

        public IndentType IndentType {
            get => (IndentType)Storage.Get(FormatterIndentTypeKey, (int)IndentType.Spaces);
            set => WritableStorage?.Set(FormatterIndentTypeKey, (int)value);
        }

        public int TabSize {
            get => Storage.Get(FormatterTabSizeKey, 4);
            set => WritableStorage?.Set(FormatterTabSizeKey, value);
        }

        public IndentStyle IndentStyle {
            get => (IndentStyle)Storage.Get(IndentStyleKey, (int)IndentStyle.Smart);
            set => WritableStorage?.Set(IndentStyleKey, (int)value);
        }

        public bool SyntaxCheckEnabled {
            get => Storage.Get(SyntaxCheckKey, true);
            set => WritableStorage?.Set(SyntaxCheckKey, value);
        }

        public bool SignatureHelpEnabled {
            get => Storage.Get(SignatureHelpEnabledKey, true);
            set => WritableStorage?.Set(SignatureHelpEnabledKey, value);
        }

        public bool InsertMatchingBraces {
            get => Storage.Get(InsertMatchingBracesKey, true);
            set => WritableStorage?.Set(InsertMatchingBracesKey, value);
        }
    }
}
