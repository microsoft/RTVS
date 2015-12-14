using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Shell {
    /// <summary>
    /// Provides abstraction of application services to editor components
    /// </summary>
    public sealed class EditorShell {
        private static Dictionary<string, ISettingsStorage> _settingStorageMap = new Dictionary<string, ISettingsStorage>(StringComparer.OrdinalIgnoreCase);
        private static object _shell;
        private static readonly object _lock = new object();

        public void SetShell(object shell) {
            _shell = shell;
         }

        public static bool HasShell {
            get { return _shell != null; }
        }

        public static void Init() {
        }

        public static IEditorShell Current {
            get {
                lock (_lock) {
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

        public static bool IsUIThread {
            get { return Current != null ? Current.MainThread == Thread.CurrentThread : true; }
        }

        /// <summary>
        /// Provides a way to execute action on UI thread while
        /// UI thread is waiting for the completion of the action.
        /// May be implemented using ThreadHelper in VS or via
        /// SynchronizationContext in all-managed application.
        /// </summary>
        /// <param name="action">Delegate to execute</param>
        /// <param name="arguments">Arguments to pass to the delegate</param>
        public static void DispatchOnUIThread(Action action) {
            if (Current != null) {
                Current.DispatchOnUIThread(action);
            } else {
                action();
            }
        }

        public static ISettingsStorage GetSettings(string contentTypeName) {
            ISettingsStorage settingsStorage = null;

            lock (_lock) {
                if (_settingStorageMap.TryGetValue(contentTypeName, out settingsStorage)) {
                    return settingsStorage;
                }
            }

            // Need to find the settings using MEF (don't use MEF inside of other locks, that can lead to deadlock)

            var contentTypeRegistry = Current.ExportProvider.GetExportedValue<IContentTypeRegistryService>();

            var contentType = contentTypeRegistry.GetContentType(contentTypeName);
            Debug.Assert(contentType != null, "Cannot find content type object for " + contentTypeName);

            settingsStorage = ComponentLocatorForOrderedContentType<IWritableSettingsStorage>.FindFirstOrderedComponent(contentType);

            if (settingsStorage == null) {
                settingsStorage = ComponentLocatorForOrderedContentType<ISettingsStorage>.FindFirstOrderedComponent(contentType);
            }

            if (settingsStorage == null) {
                var storages = ComponentLocatorForContentType<IWritableSettingsStorage, IComponentContentTypes>.ImportMany(contentType);
                if (storages.Count() > 0)
                    settingsStorage = storages.First().Value;
            }

            if (settingsStorage == null) {
                var readonlyStorages = ComponentLocatorForContentType<ISettingsStorage, IComponentContentTypes>.ImportMany(contentType);
                if (readonlyStorages.Count() > 0)
                    settingsStorage = readonlyStorages.First().Value;
            }

            Debug.Assert(settingsStorage != null, String.Format(CultureInfo.CurrentCulture,
                "Cannot find settings storage export for content type '{0}'", contentTypeName));

            lock (_lock) {
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
