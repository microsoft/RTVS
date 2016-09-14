// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Shell {
    /// <summary>
    /// Provides abstraction of application services to editor components
    /// </summary>
    public static class EditorShell {
        private static Dictionary<string, ISettingsStorage> _settingStorageMap = new Dictionary<string, ISettingsStorage>(StringComparer.OrdinalIgnoreCase);
        private static object _shell;
        private static readonly object _instanceLock = new object();
        private static readonly object _settingsLock = new object();

        public static bool HasShell  => _shell != null;

        public static IEditorShell Current {
            get {
                lock (_instanceLock) {
                    if (_shell == null) {
                        CoreShell.TryCreateTestInstance("Microsoft.Languages.Editor.Test.dll", "TestEditorShell");
                        Debug.Assert(_shell != null);
                    }

                    return _shell as IEditorShell;
                }
            }
            internal set {
                _shell = value;
            }
        }

        public static ISettingsStorage GetSettings(ICompositionCatalog compositionCatalog, string contentTypeName) {
            ISettingsStorage settingsStorage = null;

            lock (_settingsLock) {
                if (_settingStorageMap.TryGetValue(contentTypeName, out settingsStorage)) {
                    return settingsStorage;
                }

                // Need to find the settings using MEF (don't use MEF inside of other locks, that can lead to deadlock)

            var contentTypeRegistry = compositionCatalog.ExportProvider.GetExportedValue<IContentTypeRegistryService>();

                var contentType = contentTypeRegistry.GetContentType(contentTypeName);
                Debug.Assert(contentType != null, "Cannot find content type object for " + contentTypeName);

            settingsStorage = ComponentLocatorForOrderedContentType<IWritableSettingsStorage>.FindFirstOrderedComponent(compositionCatalog.CompositionService, contentType);

            if (settingsStorage == null) {
                settingsStorage = ComponentLocatorForOrderedContentType<ISettingsStorage>.FindFirstOrderedComponent(compositionCatalog.CompositionService, contentType);
            }

            if (settingsStorage == null) {
                var storages = ComponentLocatorForContentType<IWritableSettingsStorage, IComponentContentTypes>.ImportMany(compositionCatalog.CompositionService, contentType);
                if (storages.Any())
                    settingsStorage = storages.First().Value;
            }

            if (settingsStorage == null) {
                var readonlyStorages = ComponentLocatorForContentType<ISettingsStorage, IComponentContentTypes>.ImportMany(compositionCatalog.CompositionService, contentType);
                if (readonlyStorages.Any())
                    settingsStorage = readonlyStorages.First().Value;
            }

                Debug.Assert(settingsStorage != null, String.Format(CultureInfo.CurrentCulture,
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
