// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Settings {
    public static class CommonSettings {
        public const string AutoFormatKey = "AutoFormat";
        public const string CompletionEnabledKey = "CompletionEnabled";
        public const string FormatterIndentSizeKey = "FormatterIndentSize";
        public const string FormatterTabSizeKey = "FormatterTabSize";
        public const string FormatterIndentTypeKey = "FormatterIndentType";
        public const string IndentStyleKey = "IndentStyle";
        public const string InsertMatchingBracesKey = "InsertMatchingBraces";
        public const string SignatureHelpEnabledKey = "SignatureHelpEnabled";
        public const string ValidationEnabledKey = "ValidationEnabled";
        public const string ShowInternalFunctionsKey = "ShowInternalFunctions";
        public const string ShowTclFunctionsKey = "ShowTclFunctions";

        private static readonly Dictionary<string, IEditorSettingsStorage> _settingStorageMap = new Dictionary<string, IEditorSettingsStorage>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _settingsLock = new object();

        public static bool GetAutoFormat(this IEditorSettingsStorage storage) => storage.GetBoolean(AutoFormatKey, true);
        public static bool GetCompletionEnabled(this IEditorSettingsStorage storage) => storage.GetBoolean(CompletionEnabledKey, true);
        public static int GetFormatterIndentSize(this IEditorSettingsStorage storage) => storage.GetInteger(FormatterIndentSizeKey, 4);
        public static IndentType GetFormatterIndentType(this IEditorSettingsStorage storage) => (IndentType)storage.GetInteger(FormatterIndentTypeKey, (int)IndentType.Spaces);
        public static int GetFormatterTabSize(this IEditorSettingsStorage storage)=> storage.GetInteger(FormatterTabSizeKey, 4);
        public static IndentStyle GetIndentStyle(this IEditorSettingsStorage storage)=> (IndentStyle)storage.GetInteger(IndentStyleKey, (int)IndentStyle.Smart);
        public static bool GetSignatureHelpEnabled(this IEditorSettingsStorage storage)=> storage.GetBoolean(SignatureHelpEnabledKey, true);
        public static bool GetValidationEnabled(this IEditorSettingsStorage storage)=> storage.GetBoolean(ValidationEnabledKey, true);

        public static IEditorSettingsStorage GetSettingsStorage(ICompositionCatalog compositionCatalog, string contentTypeName) {
            IEditorSettingsStorage settingsStorage = null;

            lock (_settingsLock) {
                if (_settingStorageMap.TryGetValue(contentTypeName, out settingsStorage)) {
                    return settingsStorage;
                }

                // Need to find the settings using MEF (don't use MEF inside of other locks, that can lead to deadlock)
                var contentTypeRegistry = compositionCatalog.ExportProvider.GetExportedValue<IContentTypeRegistryService>();

                var contentType = contentTypeRegistry.GetContentType(contentTypeName);
                Debug.Assert(contentType != null, "Cannot find content type object for " + contentTypeName);

                var cs = compositionCatalog.CompositionService;
                settingsStorage = ComponentLocatorForOrderedContentType<IWritableEditorSettingsStorage>.FindFirstOrderedComponent(cs, contentType);

                if (settingsStorage == null) {
                    settingsStorage = ComponentLocatorForOrderedContentType<IEditorSettingsStorage>.FindFirstOrderedComponent(cs, contentType);
                }

                if (settingsStorage == null) {
                    var storages = ComponentLocatorForContentType<IWritableEditorSettingsStorage, IComponentContentTypes>.ImportMany(cs, contentType);
                    if (storages.Any()) {
                        settingsStorage = storages.First().Value;
                    }
                }

                if (settingsStorage == null) {
                    var readonlyStorages = ComponentLocatorForContentType<IEditorSettingsStorage, IComponentContentTypes>.ImportMany(cs, contentType);
                    if (readonlyStorages.Any()) {
                        settingsStorage = readonlyStorages.First().Value;
                    }
                }

                Debug.Assert(settingsStorage != null, String.Format(CultureInfo.InvariantCulture,
                    "Cannot find settings storage export for content type '{0}'", contentTypeName));

                if (_settingStorageMap.ContainsKey(contentTypeName)) {
                    // some other thread came along and loaded settings already
                    settingsStorage = _settingStorageMap[contentTypeName];
                } else {
                    _settingStorageMap[contentTypeName] = settingsStorage;
                    settingsStorage.LoadFromStorage();
                }
            }

            return settingsStorage;
        }
    }
}
