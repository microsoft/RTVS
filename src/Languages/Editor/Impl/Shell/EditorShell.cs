using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Composition;

namespace Microsoft.Languages.Editor.Shell
{
    /// <summary>
    /// A static class that provides common services to all Web Editing components
    /// </summary>
    public static class EditorShell
    {
        // Cached properties that can be accessed after the IEditorShell goes away
        public static string HostUserFolder { get; private set; }
        public static int HostLocaleId { get; private set; }

        private static Dictionary<string, IEditorSettingsStorage> _settingStorageMap = new Dictionary<string, IEditorSettingsStorage>(StringComparer.OrdinalIgnoreCase);
        private static object _lock = new object();
        private static IEditorShell _shell;

        private static List<EventHandler<EventArgs>> _onIdleHandlers = new List<EventHandler<EventArgs>>();

        // Used for cyclic OnIdle notifications
        private static int _currentIdleHandlerIndex = 0;

        /// <summary>
        /// Fires when application goes idle
        /// </summary>
        public static event EventHandler<EventArgs> OnIdle
        {
            add
            {
                _onIdleHandlers.Add(value);
            }

            remove
            {
                int foundIndex = _onIdleHandlers.IndexOf(value);
                if (foundIndex >= 0)
                {
                    _onIdleHandlers.RemoveAt(foundIndex);
                    if (_currentIdleHandlerIndex > foundIndex)
                    {
                        _currentIdleHandlerIndex--;
                    }
                }
            }
        }

        /// <summary>
        /// Fires when application terminates
        /// </summary>
        public static event EventHandler<EventArgs> OnTerminate;

        /// <summary>
        /// Web editor host application
        /// </summary>
        public static IEditorShell Shell
        {
            get
            {
                if (_shell == null)
                {
                    lock (_lock)
                    {
                        if (_shell == null)
                        {
                            // Fall back to VS case. If code is being reused in another IDE, the host application
                            // should have called SetHost by now. If it didn't this is either VS or there is a bug.

                            string assemblyName = typeof(EditorShell).Assembly.FullName.Replace("Microsoft.Languages.Editor", "Microsoft.VisualStudio.R.Package");
                            System.Reflection.Assembly package = System.Reflection.Assembly.Load(assemblyName);
                            Debug.Assert(package != null);

                            IEditorShell shell = package.CreateInstance("Microsoft.VisualStudio.R.Package.Shell.VsEditorShell") as IEditorShell;
                            Debug.Assert(shell != null);

                            SetShell(shell);
                        }
                    }
                }

                return _shell;
            }
        }

        public static bool HasShell
        {
            get { return _shell != null; }
        }

        /// <summary>
        /// Composition service provided by the host
        /// </summary>
        public static ICompositionService CompositionService
        {
            get { return Shell.CompositionService; }
        }

        /// <summary>
        /// ExportProvider provided by the host
        /// </summary>
        public static ExportProvider ExportProvider
        {
            get { return Shell.ExportProvider; }
        }

        public static Thread UIThread { get; set; }

        public static bool IsUIThread
        {
            get { return UIThread == Thread.CurrentThread; }
        }

        /// <summary>
        /// Composition service provided by the host
        /// </summary>
        public static ICommandTarget TranslateCommandTarget(ITextView textView, object target)
        {
            return Shell.TranslateCommandTarget(textView, target);
        }

        public static void DispatchOnUIThread(Action action)
        {
            DispatchOnUIThread(action, DispatcherPriority.Normal);
        }

        /// <summary>
        /// Composition service provided by the host
        /// </summary>
        public static object TranslateToHostCommandTarget(ITextView textView, object target)
        {
            return Shell.TranslateToHostCommandTarget(textView, target);
        }

        /// <summary>
        /// Provides a way to execute action on UI thread while
        /// UI thread is waiting for the completion of the action.
        /// May be implemented using ThreadHelper in VS or via
        /// SynchronizationContext in all-managed application.
        /// </summary>
        /// <param name="action">Delegate to execute</param>
        /// <param name="arguments">Arguments to pass to the delegate</param>
        public static void DispatchOnUIThread(Action action, DispatcherPriority priority)
        {
            if (UIThread != null)
            {
                var dispatcher = Dispatcher.FromThread(UIThread);

                Debug.Assert(dispatcher != null);

                if (dispatcher != null && !dispatcher.HasShutdownStarted)
                    dispatcher.BeginInvoke(action, priority);
            }
            else if (HasShell) // Can be null in unit tests
            {
                Shell.DispatchOnUIThread(action, priority);
            }
            else
            {
                action();
            }
        }

        public static bool ShowHelp(string topicName)
        {
            return Shell.ShowHelp(topicName);
        }

        public static IEditorSettingsStorage GetSettings(string contentTypeName)
        {
            IEditorSettingsStorage settingsStorage = null;

            lock (_lock)
            {
                if (_settingStorageMap.TryGetValue(contentTypeName, out settingsStorage))
                {
                    return settingsStorage;
                }
            }

            // Need to find the settings using MEF (don't use MEF inside of other locks, that can lead to deadlock)

            var contentTypeRegistry = EditorShell.ExportProvider.GetExport<IContentTypeRegistryService>().Value;
            var contentType = contentTypeRegistry.GetContentType(contentTypeName);

            settingsStorage = ComponentLocatorForOrderedContentType<IWritableEditorSettingsStorage>.FindFirstOrderedComponent(contentType);

            if (settingsStorage == null)
            {
                settingsStorage = ComponentLocatorForOrderedContentType<IEditorSettingsStorage>.FindFirstOrderedComponent(contentType);
            }

            if (settingsStorage == null)
            {
                var storages = ComponentLocatorForContentType<IWritableEditorSettingsStorage, IComponentContentTypes>.ImportMany(contentType);
                if (storages.Count() > 0)
                    settingsStorage = storages.First().Value;
            }

            if (settingsStorage == null)
            {
                var readonlyStorages = ComponentLocatorForContentType<IEditorSettingsStorage, IComponentContentTypes>.ImportMany(contentType);
                if (readonlyStorages.Count() > 0)
                    settingsStorage = readonlyStorages.First().Value;
            }

            Debug.Assert(settingsStorage != null, String.Format(CultureInfo.CurrentCulture,
                "Cannot find settings storage export for content type '{0}'", contentTypeName));

            lock (_lock)
            {
                if (_settingStorageMap.ContainsKey(contentTypeName))
                {
                    // some other thread came along and loaded settings already
                    settingsStorage = _settingStorageMap[contentTypeName];
                }
                else
                {
                    _settingStorageMap[contentTypeName] = settingsStorage;
                    settingsStorage.LoadFromStorage();
                }
            }

            return settingsStorage;
        }

        public static void SetShell(IEditorShell shell)
        {
            lock (_lock)
            {
                if (shell == null)
                {
                    throw new ArgumentNullException("shell");
                }

                if (_shell == null)
                {
                    _shell = shell;

                    _shell.Idle += host_OnIdle;
                    _shell.Terminating += host_OnTerminate;

                    CacheHostProperties();
                }
            }
        }

        public static void RemoveShell(IEditorShell shell)
        {
            lock (_lock)
            {
                Debug.Assert(_shell != null && _shell == shell, "Trying to remove wrong editor shell");
                if (_shell == shell)
                {
                    DisposeSettings();

                    _shell.Idle -= host_OnIdle;
                    _shell.Terminating -= host_OnTerminate;
                    _shell = null;
                }
            }
        }

        private static void CacheHostProperties()
        {
            // Dev12 bug 786618 - Cache some host properties so that they can be accessed from background
            // threads even after the host has been cleaned up.

            HostUserFolder = Shell.UserFolder;
            HostLocaleId = Shell.LocaleId;
        }

        private static void DisposeSettings()
        {
            List<IEditorSettingsStorage> settings = new List<IEditorSettingsStorage>();
            lock (_lock)
            {
                settings.AddRange(_settingStorageMap.Values);
                _settingStorageMap.Clear();
            }

            foreach (IEditorSettingsStorage setting in settings)
            {
                if (setting is IDisposable)
                {
                    ((IDisposable)setting).Dispose();
                }
            }
        }

        static void host_OnIdle(object sender, EventArgs eventArgs)
        {
            DoIdle(sender, eventArgs);
        }

        internal static void DoIdle(object sender, EventArgs eventArgs)
        { 
            if (_onIdleHandlers.Count > 0)
            {
                Stopwatch sw = Stopwatch.StartNew();
                int initialIndex = _currentIdleHandlerIndex;
                while (sw.ElapsedMilliseconds < 200)
                {
                    try
                    {
                        _onIdleHandlers[_currentIdleHandlerIndex++](sender, eventArgs);
                    }
                    catch
                    {
                        // silently eat any exceptions thrown by idle handlers
                    }

                    if (_currentIdleHandlerIndex >= _onIdleHandlers.Count)
                    {
                        _currentIdleHandlerIndex = 0;
                    }

                    if (_currentIdleHandlerIndex == initialIndex)
                    {
                        // We've cycled through all idle handlers
                        break;
                    }
                }
                sw.Stop();
            }
        }

        static void host_OnTerminate(object sender, EventArgs eventArgs)
        {
            if (OnTerminate != null)
            {
                OnTerminate(sender, eventArgs);
            }
        }
    }
}
